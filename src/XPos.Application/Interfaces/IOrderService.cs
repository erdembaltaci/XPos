using XPos.Shared.DTOs;

namespace XPos.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderDto>> GetOrdersByTableAsync(string tableNumber);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task UpdateOrderStatusAsync(int id, int statusId);
    Task UpdateOrderItemStatusAsync(int orderItemId, int statusId);
    Task<IEnumerable<OrderItemDto>> GetActiveItemsByStationAsync(int stationId);
    Task AddPaymentAsync(int id, decimal amount);
    Task PayOrderItemsAsync(int orderId, List<int> orderItemIds);
    Task<OrderDto?> GetActiveOrderForTableAsync(string tableNumber);
}
