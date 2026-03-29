using XPos.Application.Interfaces;
using XPos.Domain.Interfaces;
using XPos.Shared.DTOs;
using XPos.Shared.Enums;

namespace XPos.Application.Services;

public class ReportService : IReportService
{
    private readonly IOrderRepository _orderRepository;

    public ReportService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<DailySalesDto>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        
        var paidOrders = orders
            .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt.Date >= startDate.Date && o.CreatedAt.Date <= endDate.Date)
            .ToList();

        var dailySales = paidOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                TotalAmount = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return dailySales;
    }

    public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int count)
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        var paidOrders = orders.Where(o => o.Status == OrderStatus.Paid);

        var allItems = paidOrders.SelectMany(o => o.Items);

        var productSales = allItems
            .GroupBy(i => new { i.ProductId, i.Product?.Name })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name ?? "Bilinmeyen Ürün",
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(count)
            .ToList();

        return productSales;
    }

    public async Task<IEnumerable<CategorySalesDto>> GetCategorySalesAsync()
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        var paidOrders = orders.Where(o => o.Status == OrderStatus.Paid);
        var allItems = paidOrders.SelectMany(o => o.Items);

        var totalRevenue = allItems.Sum(i => i.Quantity * i.UnitPrice);
        if (totalRevenue == 0) return new List<CategorySalesDto>();

        var categorySales = allItems
            .GroupBy(i => i.Product?.Category?.Name ?? "Diğer")
            .Select(g => new CategorySalesDto
            {
                CategoryName = g.Key,
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .Select(c => new CategorySalesDto
            {
                CategoryName = c.CategoryName,
                TotalRevenue = c.TotalRevenue,
                Percentage = (double)(c.TotalRevenue / totalRevenue * 100)
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

        return categorySales;
    }
}
