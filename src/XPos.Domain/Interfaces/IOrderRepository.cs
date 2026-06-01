using XPos.Shared.Entities;

namespace XPos.Domain.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync();
    Task<IEnumerable<Order>> GetOrdersByTableWithItemsAsync(string tableNumber);
    Task<Order?> GetOrderByItemIdAsync(int orderItemId);
    Task<IEnumerable<Order>> GetPaidOrdersWithItemsAsync();
    Task<IEnumerable<Order>> GetPaidOrdersWithItemsByDateAsync(System.DateTime startDate, System.DateTime endDate);
}
