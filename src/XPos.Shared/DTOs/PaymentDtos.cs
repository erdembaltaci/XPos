namespace XPos.Shared.DTOs;

public class ItemPaymentRequestDto
{
    public List<int> OrderItemIds { get; set; } = new();
    public decimal Amount { get; set; } // Optional: Amount paid for these items if needed, or calculated
}

public class PaymentRequestDto
{
    public decimal Amount { get; set; }
}
