using Microsoft.AspNetCore.Mvc;
using XPos.Application.Interfaces;
using XPos.Shared.DTOs;

namespace XPos.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tables = await _tableService.GetAllTablesAsync();
        return Ok(tables);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var table = await _tableService.GetTableByIdAsync(id);
        if (table == null) return NotFound();
        return Ok(table);
    }

    [HttpGet("by-token/{token}")]
    public async Task<IActionResult> GetByToken(Guid token)
    {
        var table = await _tableService.GetTableByTokenAsync(token);
        if (table == null) return NotFound(new { message = "Geçersiz masa tokenı." });
        return Ok(table);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TableDto tableDto)
    {
        var createdTable = await _tableService.CreateTableAsync(tableDto);
        return CreatedAtAction(nameof(GetById), new { id = createdTable.Id }, createdTable);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TableDto tableDto)
    {
        if (id != tableDto.Id && tableDto.Id != 0) return BadRequest();
        await _tableService.UpdateTableAsync(id, tableDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tableService.DeleteTableAsync(id);
        return NoContent();
    }
}
