using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using XPos.Application.Interfaces;

namespace XPos.Application.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    // Cache the result for a short period to avoid spamming the free API for every single order.
    private static (string condition, double temperature)? _cachedWeather;
    private static DateTime _lastFetchTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string condition, double temperature)> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        if (_cachedWeather.HasValue && (DateTime.Now - _lastFetchTime) < CacheDuration)
        {
            return _cachedWeather.Value;
        }

        try
        {
            // Open-Meteo API doesn't require an API key and is free for non-commercial use
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&current_weather=true";
            
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var current = jsonDoc.RootElement.GetProperty("current_weather");
                
                double temp = current.GetProperty("temperature").GetDouble();
                int weatherCode = current.GetProperty("weathercode").GetInt32();
                
                string condition = GetWeatherDescription(weatherCode);

                _cachedWeather = (condition, temp);
                _lastFetchTime = DateTime.Now;

                return _cachedWeather.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WeatherService] Error fetching weather: {ex.Message}");
        }

        return ("Bilinmiyor", 20.0); // Fallback
    }

    private string GetWeatherDescription(int code)
    {
        // WMO Weather interpretation codes
        return code switch
        {
            0 => "Güneşli",
            1 or 2 or 3 => "Bulutlu",
            45 or 48 => "Sisli",
            51 or 53 or 55 => "Çiseli",
            61 or 63 or 65 => "Yağmurlu",
            66 or 67 => "Dondurucu Yağmur",
            71 or 73 or 75 => "Karlı",
            77 => "Kar Taneleri",
            80 or 81 or 82 => "Sağanak Yağışlı",
            85 or 86 => "Yoğun Kar",
            95 => "Gök Gürültülü Fırtına",
            96 or 99 => "Dolu Fırtınası",
            _ => "Bilinmiyor"
        };
    }
}
