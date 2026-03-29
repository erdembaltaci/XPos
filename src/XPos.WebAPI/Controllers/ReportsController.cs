using Microsoft.AspNetCore.Mvc;
using XPos.Application.Interfaces;

namespace XPos.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate == default) startDate = DateTime.Today.AddDays(-7);
        if (endDate == default) endDate = DateTime.Today;

        var result = await _reportService.GetDailySalesAsync(startDate, endDate);
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5)
    {
        var result = await _reportService.GetTopSellingProductsAsync(count);
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategorySales()
    {
        var result = await _reportService.GetCategorySalesAsync();
        return Ok(result);
    }
}
