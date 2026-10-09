using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinalYearProject
{
    public sealed class WeatherService
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m";
            using var response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var weather = await JsonSerializer.DeserializeAsync<OpenMeteoResponse>(stream);
            if (weather?.Current?.Temperature is not double temperature)
            {
                throw new InvalidDataException("The weather service response did not include a temperature.");
            }

            return new WeatherData(temperature);
        }

        private sealed class OpenMeteoResponse
        {
            [JsonPropertyName("current")]
            public CurrentWeather? Current { get; set; }
        }

        private sealed class CurrentWeather
        {
            [JsonPropertyName("temperature_2m")]
            public double? Temperature { get; set; }
        }
    }

    public sealed record WeatherData(double Temperature);
}
