using Microsoft.EntityFrameworkCore;
using XPos.Domain.Interfaces;
using XPos.Infrastructure.Persistence;
using XPos.Shared.Entities;
using XPos.Shared.Enums;

namespace XPos.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(XPosDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .OrderByDescending(o => o.CreatedAt)
            .Take(150)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByTableWithItemsAsync(string tableNumber)
    {
        var trimmed = tableNumber.Trim();
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .Where(o => o.TableNumber == trimmed && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByItemIdAsync(int orderItemId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Items.Any(i => i.Id == orderItemId));
    }

    public async Task<IEnumerable<Order>> GetPaidOrdersWithItemsAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .Where(o => o.Status == OrderStatus.Paid)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetPaidOrdersWithItemsByDateAsync(System.DateTime startDate, System.DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .AsSplitQuery()
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= start && o.CreatedAt < end.AddDays(1))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
