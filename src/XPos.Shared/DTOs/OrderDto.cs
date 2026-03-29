using XPos.Shared.Enums;

namespace XPos.Shared.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}



