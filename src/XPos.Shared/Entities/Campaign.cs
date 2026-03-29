namespace XPos.Shared.Entities;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DiscountRate { get; set; }
    public int? RequiredProductId { get; set; }
    public int? TargetProductId { get; set; }
    public bool IsActive { get; set; } = true;
}
