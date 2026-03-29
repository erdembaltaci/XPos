using System;
using System.ComponentModel.DataAnnotations;

namespace XPos.Shared.DTOs;

public class TableDto
{
    public int Id { get; set; }
    
    [Required]
    public string TableNumber { get; set; } = string.Empty;
    
    public int Capacity { get; set; }
    
    public bool IsOccupied { get; set; } // Dolu mu?
    public bool HasPendingOrder { get; set; } // Onay bekleyen sipariş var mı?
    public int? CurrentOrderId { get; set; }
    public decimal? CurrentOrderAmount { get; set; }
    public Guid SecretToken { get; set; }
}
