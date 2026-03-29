using XPos.Shared.DTOs;
using XPos.Application.Interfaces;
using XPos.Domain.Interfaces;
using XPos.Shared.Entities;
using XPos.Shared.Enums;

namespace XPos.Application.Services;

public class TableService : ITableService
{
    private readonly IGenericRepository<Table> _tableRepository;
    private readonly IOrderRepository _orderRepository;

    public TableService(IGenericRepository<Table> tableRepository, IOrderRepository orderRepository)
    {
        _tableRepository = tableRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<TableDto>> GetAllTablesAsync()
    {
        var tables = await _tableRepository.GetAllAsync();
        var activeOrders = await _orderRepository.FindAsync(o => o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);
        
        return tables.Select(t => 
        {
            var tableOrders = activeOrders.Where(o => o.TableNumber == t.TableNumber).ToList();
            
            // Eğer birden fazla aktif sipariş varsa, bunları toplam olarak göster
            var hasActive = tableOrders.Any();
            var hasPending = tableOrders.Any(o => o.Status == OrderStatus.Pending);
            var totalAmount = tableOrders.Sum(o => o.TotalAmount);
            var oldestOrder = tableOrders.OrderBy(o => o.CreatedAt).FirstOrDefault();

            return new TableDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                IsOccupied = hasActive,
                HasPendingOrder = hasPending,
                CurrentOrderId = oldestOrder?.Id,
                CurrentOrderAmount = totalAmount,
                SecretToken = t.SecretToken
            };
        }).ToList();
    }
    public async Task<TableDto?> GetTableByIdAsync(int id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table == null) return null;

        return new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            IsOccupied = table.Status == TableStatus.Occupied,
            SecretToken = table.SecretToken
        };
    }

    public async Task<TableDto?> GetTableByTokenAsync(Guid token)
    {
        var tables = await _tableRepository.FindAsync(t => t.SecretToken == token);
        var table = tables.FirstOrDefault();
        if (table == null) return null;

        return new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            IsOccupied = table.Status == TableStatus.Occupied,
            SecretToken = table.SecretToken
            // Note: Occupancy check via OrderRepository is skipped here for simplicity,
            // or we could fetch active orders if needed. Ideally consistency is better.
        };
    }

    public async Task<TableDto> CreateTableAsync(TableDto tableDto)
    {
        var table = new Table
        {
            TableNumber = tableDto.TableNumber,
            Capacity = tableDto.Capacity,
            Status = TableStatus.Available
        };

        await _tableRepository.AddAsync(table);
        
        tableDto.Id = table.Id;
        return tableDto; // ID ile döner
    }
    
    public async Task UpdateTableAsync(int id, TableDto tableDto)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table != null)
        {
            table.TableNumber = tableDto.TableNumber;
            table.Capacity = tableDto.Capacity;
            // Status'u burada değiştirmiyoruz, ayrı metod var
            await _tableRepository.UpdateAsync(table);
        }
    }

    public async Task DeleteTableAsync(int id)
    {
        await _tableRepository.DeleteAsync(id);
    }
    
    public async Task UpdateTableStatusAsync(int id, bool isOccupied)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table != null)
        {
            table.Status = isOccupied ? TableStatus.Occupied : TableStatus.Available;
            await _tableRepository.UpdateAsync(table);
        }
    }
}
