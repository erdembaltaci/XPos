using XPos.Shared.Enums;

namespace XPos.Shared.Entities;

public class Order
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int? WaiterId { get; set; }
    
    public decimal PaidAmount { get; set; } = 0;
    
    // AI verisi için
    public string WeatherCondition { get; set; } = string.Empty;
    public double? Temperature { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
