using Microsoft.AspNetCore.Mvc;
using XPos.Shared.DTOs;
using XPos.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using XPos.WebAPI.Hubs;

namespace XPos.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<OrderHub> _hubContext;

    public OrdersController(IOrderService orderService, IHubContext<OrderHub> hubContext)
    {
        _orderService = orderService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("table/{tableNumber}")]
    public async Task<IActionResult> GetByTable(string tableNumber)
    {
        var orders = await _orderService.GetOrdersByTableAsync(tableNumber);
        return Ok(orders);
    }

    [HttpGet("active/{tableNumber}")]
    public async Task<IActionResult> GetActiveByTable(string tableNumber)
    {
        var order = await _orderService.GetActiveOrderForTableAsync(tableNumber);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto createOrderDto)
    {
        try
        {
            Console.WriteLine($"[API] Sipariş isteği alındı. Masa: {createOrderDto.TableNumber}, Ürün Sayısı: {createOrderDto.Items.Count}");
            
            var createdOrder = await _orderService.CreateOrderAsync(createOrderDto);
            Console.WriteLine($"[API] Sipariş veritabanına kaydedildi. ID: {createdOrder.Id}");

            try
            {
                // SignalR ile tüm bağlı istemcilere bildir
                await _hubContext.Clients.All.SendAsync("OrderReceived", createdOrder);
                Console.WriteLine("[API] SignalR bildirimi gönderildi.");
            }
            catch (Exception hubEx)
            {
                Console.WriteLine($"[API-ERROR] SignalR Hatası: {hubEx.Message}");
            }
            
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Sunucu hatası: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $" | Detay: {ex.InnerException.Message}";
                Console.WriteLine($"[API-FATAL-INNER] {ex.InnerException.Message}");
            }

            Console.WriteLine($"[API-FATAL] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, errorMsg);
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int statusId)
    {
        await _orderService.UpdateOrderStatusAsync(id, statusId);
        
        try
        {
            var updatedOrder = await _orderService.GetOrderByIdAsync(id);
            if (updatedOrder != null)
            {
                await _hubContext.Clients.All.SendAsync("OrderReceived", updatedOrder);
            }
        }
        catch (Exception hubEx)
        {
            Console.WriteLine($"[API-ERROR] SignalR Status Update Hatası: {hubEx.Message}");
        }

        return NoContent();
    }

    [HttpPut("item/{orderItemId}/status")]
    public async Task<IActionResult> UpdateItemStatus(int orderItemId, [FromBody] int statusId)
    {
        await _orderService.UpdateOrderItemStatusAsync(orderItemId, statusId);
        
        try
        {
            // Kalem güncellendiğinde siparişi de bildirmek için order ID'yi bulalım
            var updatedOrder = await _orderService.GetOrderByItemIdAsync(orderItemId);
            if (updatedOrder != null)
            {
                await _hubContext.Clients.All.SendAsync("OrderReceived", updatedOrder);
            }
        }
        catch (Exception hubEx)
        {
            Console.WriteLine($"[API-ERROR] SignalR Item Status Update Hatası: {hubEx.Message}");
        }

        return NoContent();
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> AddPayment(int id, [FromBody] PaymentRequestDto request)
    {
        await _orderService.AddPaymentAsync(id, request.Amount);
        
        try
        {
            var updatedOrder = await _orderService.GetOrderByIdAsync(id);
            if (updatedOrder != null)
            {
                await _hubContext.Clients.All.SendAsync("OrderReceived", updatedOrder);
            }
        }
        catch (Exception hubEx)
        {
            Console.WriteLine($"[API-ERROR] SignalR Payment Hatası: {hubEx.Message}");
        }

        return NoContent();
    }

    [HttpPost("{id}/payment/items")]
    public async Task<IActionResult> PayOrderItems(int id, [FromBody] ItemPaymentRequestDto request)
    {
        await _orderService.PayOrderItemsAsync(id, request.OrderItemIds);
        
        try
        {
            var updatedOrder = await _orderService.GetOrderByIdAsync(id);
            if (updatedOrder != null)
            {
                await _hubContext.Clients.All.SendAsync("OrderReceived", updatedOrder);
            }
        }
        catch (Exception hubEx)
        {
            Console.WriteLine($"[API-ERROR] SignalR Item Payment Hatası: {hubEx.Message}");
        }

        return NoContent();
    }
}
