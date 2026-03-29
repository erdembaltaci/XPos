namespace XPos.Shared.DTOs;

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrderCount { get; set; }
}

public class ProductSalesDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CategorySalesDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public double Percentage { get; set; }
}
