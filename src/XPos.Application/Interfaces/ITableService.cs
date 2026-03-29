using XPos.Shared.DTOs;

namespace XPos.Application.Interfaces;

public interface ITableService
{
    Task<IEnumerable<TableDto>> GetAllTablesAsync();
    Task<TableDto?> GetTableByIdAsync(int id);
    Task<TableDto?> GetTableByTokenAsync(Guid token);
    Task<TableDto> CreateTableAsync(TableDto tableDto);
    Task UpdateTableAsync(int id, TableDto tableDto);
    Task DeleteTableAsync(int id);
    
    // Masa durumunu güncelleme (Dolu/Boş)
    Task UpdateTableStatusAsync(int id, bool isOccupied);
}
