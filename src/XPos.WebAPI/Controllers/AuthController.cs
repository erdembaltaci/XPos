using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XPos.Infrastructure.Persistence;
using XPos.Shared.DTOs;
using BCrypt.Net;

namespace XPos.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly XPosDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(XPosDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    /// <summary>
    /// Kullanıcı adı (veya telefon) + şifre ile giriş yapar, JWT token döner.
    /// Demo hesaplar:
    ///   Admin:    Username="admin"      Password="admin123"
    ///   SubAdmin: Username="subadmin"   Password="sub123"
    ///   Waiter:   Username="ali"        Password="1234"
    ///   (Telefon numarası ile de giriş yapılabilir)
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        // Username veya Phone alanından hangisi doluysa onu kullan
        var identifier = !string.IsNullOrWhiteSpace(dto.Username) ? dto.Username
                       : !string.IsNullOrWhiteSpace(dto.Phone) ? dto.Phone
                       : null;

        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(dto.AuthPassword))
            return BadRequest("Kullanıcı adı ve şifre zorunludur.");

        // Önce Username ile, sonra Phone ile ara
        var staff = await _context.Staffs
            .FirstOrDefaultAsync(s => (s.Username == identifier || s.Phone == identifier) && s.IsActive);

        if (staff == null)
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

        // BCrypt hash doğrulama
        bool passwordValid;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(dto.AuthPassword, staff.PasswordHash);
        }
        catch
        {
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");
        }

        if (!passwordValid)
            return Unauthorized("Geçersiz kullanıcı adı veya şifre.");

        var token = GenerateJwtToken(staff.Id, staff.Phone, staff.Role, $"{staff.Name} {staff.Surname}");

        return Ok(new AuthResponseDto
        {
            Token = token,
            FullName = $"{staff.Name} {staff.Surname}",
            Role = staff.Role,
            StaffId = staff.Id
        });
    }

    private string GenerateJwtToken(int staffId, string phone, string role, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiresInHours"] ?? "12"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staffId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, phone),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role, role),
            new Claim("staffId", staffId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
