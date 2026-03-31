using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XPos.Infrastructure.Persistence;
using XPos.Shared.DTOs;
using XPos.Shared.Entities;

namespace XPos.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]

public class StaffController : ControllerBase
{
    private readonly XPosDbContext _context;

    public StaffController(XPosDbContext context)
    {
        _context = context;
    }

    [HttpGet]

    public async Task<ActionResult<List<StaffDto>>> GetStaffs()
    {
        return await _context.Staffs
            .Select(s => new StaffDto
            {
                Id = s.Id,
                Name = s.Name,
                Surname = s.Surname,
                Username = s.Username,
                Phone = s.Phone,
                Role = s.Role,
                IsActive = s.IsActive
                // PasswordHash asla döndürülmüyor!
            })
            .ToListAsync();
    }

    [HttpPost]

    public async Task<ActionResult<StaffDto>> CreateStaff(CreateStaffDto dto)
    {
        if (dto.Phone?.Length != 10 || dto.Phone.StartsWith("0"))
            return BadRequest("Telefon 10 haneli ve basinda 0 olmadan olmalidir.");
        if (dto.StaffPassword?.Length != 4)
            return BadRequest("Sifre tam olarak 4 haneli olmalidir.");

        var staff = new Staff
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Username = dto.Username,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.StaffPassword),
            Role = dto.Role,
            IsActive = true
        };

        _context.Staffs.Add(staff);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStaffs), new { id = staff.Id }, new StaffDto
        {
            Id = staff.Id,
            Name = staff.Name,
            Surname = staff.Surname,
            Username = staff.Username,
            Phone = staff.Phone,
            Role = staff.Role,
            IsActive = staff.IsActive
        });
    }

    [HttpPut("{id}")]

    public async Task<IActionResult> UpdateStaff(int id, CreateStaffDto dto)
    {
        if (dto.Phone?.Length != 10 || dto.Phone.StartsWith("0"))
            return BadRequest("Telefon 10 haneli ve basinda 0 olmadan olmalidir.");
        if (!string.IsNullOrWhiteSpace(dto.StaffPassword) && dto.StaffPassword.Length != 4)
            return BadRequest("Sifre tam olarak 4 haneli olmalidir.");

        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null) return NotFound();

        staff.Name = dto.Name;
        staff.Surname = dto.Surname;
        staff.Username = dto.Username;
        staff.Phone = dto.Phone;
        staff.Role = dto.Role;

        // Şifre güncelleme: boş değilse yeni hash oluştur
        if (!string.IsNullOrWhiteSpace(dto.StaffPassword))
            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.StaffPassword);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]

    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null) return NotFound();

        // Soft delete - sadece deaktif et
        staff.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

