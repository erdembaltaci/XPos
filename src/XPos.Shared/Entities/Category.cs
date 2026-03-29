namespace XPos.Shared.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int StationId { get; set; } // Hangi istasyona gidecek
}
