using Microsoft.EntityFrameworkCore;
using XPos.Domain.Interfaces;
using XPos.Infrastructure.Persistence;
using XPos.Shared.Entities;

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
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByTableWithItemsAsync(string tableNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .Where(o => o.TableNumber == tableNumber)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByItemIdAsync(int orderItemId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(o => o.Items.Any(i => i.Id == orderItemId));
    }
}
