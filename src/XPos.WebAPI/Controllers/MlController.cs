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

    [HttpPost("recommendations/basket")]
    public async Task<IActionResult> GetBasketRecommendations([FromBody] JsonElement request)
    {
        try
        {
            // Case-insensitive check for Products and Limit
            JsonElement productsProperty = default;
            int limit = 6;

            foreach (var prop in request.EnumerateObject())
            {
                if (prop.Name.Equals("products", StringComparison.OrdinalIgnoreCase))
                    productsProperty = prop.Value;
                else if (prop.Name.Equals("limit", StringComparison.OrdinalIgnoreCase))
                    limit = prop.Value.GetInt32();
            }

            if (productsProperty.ValueKind == JsonValueKind.Array)
            {
                var basketProductNames = productsProperty.EnumerateArray()
                    .Select(p => p.GetString())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToHashSet();

                // Get all products to build the ML request with full objects
                var allProducts = await _productService.GetAllProductsAsync();
                
                var cartProducts = allProducts
                    .Where(p => basketProductNames.Contains(p.Name))
                    .Select(p => new { id = p.Id, name = p.Name, price = (double)p.Price, categoryId = p.CategoryId })
                    .ToList();
                    
                var availableProducts = allProducts
                    .Select(p => new { id = p.Id, name = p.Name, price = (double)p.Price, categoryId = p.CategoryId })
                    .ToList();

                var mlPayload = new
                {
                    cart_products = cartProducts,
                    available_products = availableProducts,
                    limit = limit
                };

                // Python ML servisine akıllı sepet isteği gönderiyoruz
                var response = await _mlClient.PostAsJsonAsync("api/recommendations/smart-basket", mlPayload);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
            }
            return BadRequest("Invalid request format: 'Products' array missing.");
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"ML Service unavailable: {ex.Message}");
        }
    }
}
