using System.ComponentModel.DataAnnotations;

namespace XPos.Shared.Entities;

public class Staff
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Surname { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty; // BCrypt hash
    
    public string Role { get; set; } = "Waiter"; // Admin, SubAdmin, Waiter
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
