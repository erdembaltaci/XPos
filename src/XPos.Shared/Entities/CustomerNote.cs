namespace XPos.Shared.Entities;

public class CustomerNote
{
    public int Id { get; set; }
    public string NoteContent { get; set; } = string.Empty; // "Az Pişmiş", "Buzsuz"
    public bool IsActive { get; set; } = true;
}
