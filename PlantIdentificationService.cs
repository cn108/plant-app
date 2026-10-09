using System.Net.Http.Headers;
using System.Text.Json;

namespace FinalYearProject
{
    public sealed record PlantMatch(string ScientificName, string? CommonName, double Score);

    public sealed class PlantIdentificationException(string message) : Exception(message);

    public static class PlantIdentificationService
    {
        private const string ApiKeyStorageKey = "plantnet_api_key";
        private const string Endpoint = "https://my-api.plantnet.org/v2/identify/all";
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

        public static async Task<string?> GetApiKeyAsync()
        {
            try
            {
                var stored = await SecureStorage.Default.GetAsync(ApiKeyStorageKey);
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    return stored;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reading the identification key failed: {ex.Message}");
            }

            var fromEnvironment = Environment.GetEnvironmentVariable("PLANTNET_API_KEY");
            return string.IsNullOrWhiteSpace(fromEnvironment) ? null : fromEnvironment.Trim();
        }

        public static async Task SaveApiKeyAsync(string apiKey)
        {
            await SecureStorage.Default.SetAsync(ApiKeyStorageKey, apiKey.Trim());
        }

        public static async Task<IReadOnlyList<PlantMatch>> IdentifyAsync(byte[] imageBytes, string fileName, string apiKey)
        {
            using var content = new MultipartFormDataContent();
            var image = new ByteArrayContent(imageBytes);
            image.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(fileName));
            content.Add(image, "images", string.IsNullOrWhiteSpace(fileName) ? "plant.jpg" : fileName);
            content.Add(new StringContent("auto"), "organs");

            var url = $"{Endpoint}?lang=en&nb-results=3&api-key={Uri.EscapeDataString(apiKey)}";

            HttpResponseMessage response;
            try
            {
                response = await Http.PostAsync(url, content);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                throw new PlantIdentificationException("Could not reach the identification service. Check your internet connection.");
            }

            using (response)
            {
                switch ((int)response.StatusCode)
                {
                    case 401 or 403:
                        throw new PlantIdentificationException("The identification key was rejected. Enter a valid Pl@ntNet key.");
                    case 404:
                        return [];
                    case 429:
                        throw new PlantIdentificationException("Daily identification limit reached. Try again tomorrow.");
                    case >= 400:
                        throw new PlantIdentificationException($"The identification service returned an error ({(int)response.StatusCode}).");
                }

                return ParseMatches(await response.Content.ReadAsStringAsync());
            }
        }

        public static IReadOnlyList<PlantMatch> ParseMatches(string jsonText)
        {
            using var json = JsonDocument.Parse(jsonText);

            var matches = new List<PlantMatch>();
            if (!json.RootElement.TryGetProperty("results", out var results))
            {
                return matches;
            }

            foreach (var result in results.EnumerateArray().Take(3))
            {
                var species = result.GetProperty("species");
                var scientific = species.TryGetProperty("scientificNameWithoutAuthor", out var sci)
                    ? sci.GetString() ?? "Unknown"
                    : "Unknown";

                string? common = null;
                if (species.TryGetProperty("commonNames", out var names) && names.GetArrayLength() > 0)
                {
                    common = names[0].GetString();
                }

                var score = result.TryGetProperty("score", out var s) ? s.GetDouble() : 0;
                matches.Add(new PlantMatch(scientific, common, score));
            }

            return matches;
        }

        private static string GuessContentType(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
    }
}
