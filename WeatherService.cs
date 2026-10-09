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
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&current=temperature_2m,relative_humidity_2m,wind_speed_10m,precipitation";
            using var response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var weather = await JsonSerializer.DeserializeAsync<OpenMeteoResponse>(stream);
            if (weather?.Current?.Temperature is not double temperature)
            {
                throw new InvalidDataException("The weather service response did not include a temperature.");
            }

            return new WeatherData(temperature, weather.Current.Humidity, weather.Current.Wind, weather.Current.Precipitation);
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

            [JsonPropertyName("relative_humidity_2m")]
            public double? Humidity { get; set; }

            [JsonPropertyName("wind_speed_10m")]
            public double? Wind { get; set; }

            [JsonPropertyName("precipitation")]
            public double? Precipitation { get; set; }
        }
    }

    public sealed record WeatherData(double Temperature, double? Humidity = null, double? WindKph = null, double? RainMm = null)
    {
        public string Advice =>
            RainMm is > 0.5 ? "It is raining, so skip watering outdoor plants today."
            : Temperature >= 30 ? "Very hot: water early morning or evening and shade tender plants."
            : Temperature >= 22 ? "Warm weather: check soil moisture daily and water deeply."
            : Temperature >= 10 ? "Mild conditions: ideal for planting and general garden work."
            : Temperature >= 3 ? "Cool: water sparingly and protect seedlings at night."
            : "Frost risk: cover or bring tender plants indoors.";
    }
}
