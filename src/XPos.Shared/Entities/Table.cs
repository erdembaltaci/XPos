using XPos.Shared.Enums;

namespace XPos.Shared.Entities;

public class Table
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Available;
    public string QRCodeUrl { get; set; } = string.Empty;
    public Guid SecretToken { get; set; } = Guid.NewGuid();
}
