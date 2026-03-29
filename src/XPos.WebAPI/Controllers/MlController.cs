using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using XPos.Application.Interfaces;

namespace XPos.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MlController : ControllerBase
{
    private static readonly HttpClient _mlClient = new()
    {
        BaseAddress = new Uri("http://localhost:5001"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    private readonly IProductService _productService;
    private readonly IDataSyncService _dataSyncService;

    public MlController(IProductService productService, IDataSyncService dataSyncService)
    {
        _productService = productService;
        _dataSyncService = dataSyncService;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncData()
    {
        try
        {
            await _dataSyncService.SyncAllDataToMlServiceAsync();
            try { await _mlClient.PostAsync("api/forecast/retrain", null); } catch { }
            return Ok(new { success = true, message = "Veriler başarıyla eşitlendi ve AI modelleri eğitildi." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Eşitleme hatası: {ex.Message}" });
        }
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast()
    {
        try
        {
            var response = await _mlClient.GetAsync("api/forecast?days=7");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, "ML Forecast error");
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"ML Service unavailable: {ex.Message}");
        }
    }

    [HttpGet("segments")]
    public async Task<IActionResult> GetSegments()
    {
        try
        {
            var response = await _mlClient.GetAsync("api/segments");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, "ML Segments error");
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"ML Service unavailable: {ex.Message}");
        }
    }

    [HttpGet("campaigns/suggest")]
    public async Task<IActionResult> GetCampaignSuggestions()
    {
        try
        {
            var response = await _mlClient.GetAsync("api/campaigns/suggest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return StatusCode((int)response.StatusCode, "ML Suggestions error");
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"ML Service unavailable: {ex.Message}");
        }
    }

    [HttpGet("campaigns/active")]
    public async Task<IActionResult> GetActiveCampaign()
    {
        try
        {
            var response = await _mlClient.GetAsync("api/campaigns/active");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            return Ok(new { active = false });
        }
        catch { return Ok(new { active = false }); }
    }
}
