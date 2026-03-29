namespace XPos.Shared.DTOs;

public class StaffDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Waiter";
    public string StaffPassword { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
