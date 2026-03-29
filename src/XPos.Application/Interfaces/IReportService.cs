using XPos.Shared.DTOs;

namespace XPos.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<DailySalesDto>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int count);
    Task<IEnumerable<CategorySalesDto>> GetCategorySalesAsync();
}
