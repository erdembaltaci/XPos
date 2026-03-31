using System.ComponentModel.DataAnnotations;

namespace XPos.Shared.DTOs;

public class LoginDto
{
    public string Username { get; set; } = string.Empty; // Kullanıcı adı veya telefon
    public string Phone { get; set; } = string.Empty; // Geriye uyumluluk
    public string AuthPassword { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int StaffId { get; set; }
}

public class CreateStaffDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "Telefon 10 haneli (basinda 0 olmadan) olmalidir.")]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Sifre tam olarak 4 haneli rakam olmalidir.")]
    public string StaffPassword { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Waiter";
}
