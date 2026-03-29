using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using XPos.Application.Interfaces;

namespace XPos.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly double _lat;
        private readonly double _lon;
        private readonly string _locationName;

        public WeatherController(IWeatherService weatherService, IConfiguration configuration)
        {
            _weatherService = weatherService;
            _lat = configuration.GetValue<double>("Location:Latitude", 39.9208);
            _lon = configuration.GetValue<double>("Location:Longitude", 32.8541);
            _locationName = configuration.GetValue<string>("Location:Name", "Bilinmiyor")!;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentWeather()
        {
            try
            {
                var (condition, temp) = await _weatherService.GetCurrentWeatherAsync(_lat, _lon);
                return Ok(new { Condition = condition, Temperature = temp, Location = _locationName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Hava durumu alınamadı.", error = ex.Message });
            }
        }
    }
}
