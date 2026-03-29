using XPos.Shared.Enums;

namespace XPos.Shared.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Note { get; set; } = string.Empty;
    public OrderStatus ItemStatus { get; set; } = OrderStatus.Pending; // İstasyon bazlı durum
    
    public virtual Product? Product { get; set; }
}
