using XPos.Shared.Enums;

namespace XPos.Shared.DTOs;

public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Note { get; set; } = string.Empty;
    public OrderStatus StationStatus { get; set; } = OrderStatus.Pending;
}
