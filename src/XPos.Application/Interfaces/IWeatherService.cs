using System.Threading.Tasks;

namespace XPos.Application.Interfaces;

public interface IWeatherService
{
    Task<(string condition, double temperature)> GetCurrentWeatherAsync(double latitude, double longitude);
}
