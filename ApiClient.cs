using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FinalYearProject
{
    public sealed record ApiSession(string Token, int UserId, string Username, string Role);
    public sealed record ApiReply(int Id, string Username, string Text, DateTime CreatedAt);
    public sealed record ApiPost(int Id, string Username, string Category, string Text, DateTime CreatedAt, List<ApiReply> Replies)
    {
        public string ReplySummary => Replies.Count switch { 0 => "No replies yet", 1 => "1 reply", _ => $"{Replies.Count} replies" };
        public string LatestReply => Replies.Count == 0 ? string.Empty : $"{Replies[^1].Username}: {Replies[^1].Text}";
        public bool HasReplies => Replies.Count > 0;
    }

    public sealed record ApiResult<T>(T? Value, string? Error, bool Unreachable)
    {
        public bool Ok => Error is null;
    }

    public static class ApiClient
    {
        private const string TokenKey = "api_token";
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public static string BaseUrl
        {
            get => Preferences.Get("api_base_url", DefaultBaseUrl);
            set => Preferences.Set("api_base_url", value.TrimEnd('/'));
        }

        private static string DefaultBaseUrl =>
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000";

        public static string? Token { get; private set; }
        public static bool IsSignedIn => !string.IsNullOrEmpty(Token);

        public static async Task RestoreAsync()
        {
            try { Token = await SecureStorage.Default.GetAsync(TokenKey); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Restoring token failed: {ex.Message}"); }
        }

        public static void SignOut()
        {
            Token = null;
            try { SecureStorage.Default.Remove(TokenKey); } catch { /* storage unavailable */ }
        }

        private static async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, bool auth = true)
        {
            try
            {
                using var request = new HttpRequestMessage(method, BaseUrl + path);
                if (body is not null) request.Content = JsonContent.Create(body, options: Json);
                if (auth && IsSignedIn) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                using var response = await Http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(object) || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Accepted)
                        return new ApiResult<T>(default, null, false);
                    return new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>(Json), null, false);
                }

                return new ApiResult<T>(default, await ReadErrorAsync(response), false);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new ApiResult<T>(default, "The server is unreachable.", true);
            }
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized) return "Invalid email or password, or your session expired.";
            if (response.StatusCode == HttpStatusCode.TooManyRequests) return "Too many attempts. Please wait a moment.";
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("error", out var e)) return e.GetString() ?? "Request failed.";
                if (doc.RootElement.TryGetProperty("errors", out var errs))
                    return string.Join(" ", errs.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString())));
            }
            catch (JsonException) { /* not JSON */ }
            return "Request failed.";
        }

        private static async Task<ApiResult<ApiSession>> StoreSession(ApiResult<ApiSession> result)
        {
            if (result.Ok && result.Value is not null)
            {
                Token = result.Value.Token;
                try { await SecureStorage.Default.SetAsync(TokenKey, Token); } catch { /* storage unavailable */ }
            }
            return result;
        }

        public static async Task<ApiResult<ApiSession>> LoginAsync(string email, string password) =>
            await StoreSession(await SendAsync<ApiSession>(HttpMethod.Post, "/api/auth/login", new { email, password }, false));

        public static async Task<ApiResult<ApiSession>> RegisterAsync(string username, string email, string password) =>
            await StoreSession(await SendAsync<ApiSession>(HttpMethod.Post, "/api/auth/register", new { username, email, password }, false));

        public static Task<ApiResult<List<ApiPost>>> GetPostsAsync() =>
            SendAsync<List<ApiPost>>(HttpMethod.Get, "/api/forum", auth: false);

        public static Task<ApiResult<object>> CreatePostAsync(string category, string text) =>
            SendAsync<object>(HttpMethod.Post, "/api/forum", new { category, text });

        public static Task<ApiResult<object>> ReplyAsync(int postId, string text) =>
            SendAsync<object>(HttpMethod.Post, $"/api/forum/{postId}/replies", new { text });

        public static Task<ApiResult<object>> ReportAsync(int postId, string reason) =>
            SendAsync<object>(HttpMethod.Post, $"/api/forum/{postId}/report", new { reason });

        public static async Task<ApiResult<IReadOnlyList<PlantMatch>>> IdentifyAsync(byte[] image, string fileName, string contentType)
        {
            if (!IsSignedIn) return new(null, "Sign in to use server identification.", true);
            try
            {
                using var content = new MultipartFormDataContent();
                var file = new ByteArrayContent(image);
                file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(file, "image", string.IsNullOrWhiteSpace(fileName) ? "plant.jpg" : fileName);

                using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/api/identify") { Content = content };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                using var response = await Http.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    return new(null, "Server identification is not configured.", true);
                if (!response.IsSuccessStatusCode)
                    return new(null, "Identification failed on the server.", false);

                return new(PlantIdentificationService.ParseMatches(await response.Content.ReadAsStringAsync()), null, false);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new(null, "The server is unreachable.", true);
            }
        }
    }
}
