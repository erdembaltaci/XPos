using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XPos.Infrastructure.Persistence;
using XPos.Shared.DTOs;

namespace XPos.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly XPosDbContext _context;

    public CategoriesController(XPosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                StationId = c.StationId
            })
            .ToListAsync();

        return Ok(categories);
    }
}
