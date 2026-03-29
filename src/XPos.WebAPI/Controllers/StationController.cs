using Microsoft.AspNetCore.Mvc;
using XPos.Application.Interfaces;

namespace XPos.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationController : ControllerBase
{
    private readonly IOrderService _orderService;

    public StationController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{stationId}/active-items")]
    public async Task<IActionResult> GetActiveItems(int stationId)
    {
        var items = await _orderService.GetActiveItemsByStationAsync(stationId);
        return Ok(items);
    }
}
