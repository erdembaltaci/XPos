using System.ComponentModel.DataAnnotations;

namespace XPos.Shared.DTOs;

public class CreateOrderDto
{
    [Required]
    public string TableNumber { get; set; } = string.Empty;

    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
}
