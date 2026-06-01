using XPos.Shared.DTOs;

namespace XPos.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<DailySalesDto>> GetDailySalesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int count, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<CategorySalesDto>> GetCategorySalesAsync(DateTime? startDate = null, DateTime? endDate = null);
}
