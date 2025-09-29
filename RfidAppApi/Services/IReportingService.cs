using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Interface for comprehensive reporting service
    /// </summary>
    public interface IReportingService
    {
        // Stock Movement Methods
        Task<StockMovementDto> CreateStockMovementAsync(CreateStockMovementDto movementDto, string clientCode, int? userId = null);
        Task<List<StockMovementDto>> CreateBulkStockMovementsAsync(BulkStockMovementDto bulkDto, string clientCode);
        Task<List<StockMovementDto>> GetStockMovementsAsync(ReportFilterDto filter, string clientCode);
        Task<StockMovementDto?> GetStockMovementByIdAsync(int movementId, string clientCode);

        // Daily Stock Balance Methods
        Task<DailyStockBalanceDto> GetDailyStockBalanceAsync(int productId, DateTime date, string clientCode);
        Task<List<DailyStockBalanceDto>> GetDailyStockBalancesAsync(ReportFilterDto filter, string clientCode);
        Task<DailyStockBalanceDto> CalculateDailyStockBalanceAsync(int productId, DateTime date, string clientCode);
        Task<List<DailyStockBalanceDto>> CalculateDailyStockBalancesAsync(DateTime date, string clientCode);

        // Sales Report Methods
        Task<List<SalesReportDto>> GetSalesReportAsync(ReportFilterDto filter, string clientCode);
        Task<SalesReportDto> GetSalesReportByDateAsync(DateTime date, string clientCode);
        Task<List<SalesReportDto>> GetSalesReportByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode);
        Task<SalesReportDto> GetSalesReportByBranchAsync(DateTime date, int branchId, string clientCode);
        Task<SalesReportDto> GetSalesReportByCounterAsync(DateTime date, int counterId, string clientCode);
        Task<SalesReportDto> GetSalesReportByCategoryAsync(DateTime date, int categoryId, string clientCode);

        // Stock Summary Report Methods
        Task<List<StockSummaryReportDto>> GetStockSummaryReportAsync(ReportFilterDto filter, string clientCode);
        Task<StockSummaryReportDto> GetStockSummaryByDateAsync(DateTime date, string clientCode);
        Task<StockSummaryReportDto> GetStockSummaryByBranchAsync(DateTime date, int branchId, string clientCode);
        Task<StockSummaryReportDto> GetStockSummaryByCounterAsync(DateTime date, int counterId, string clientCode);
        Task<StockSummaryReportDto> GetStockSummaryByCategoryAsync(DateTime date, int categoryId, string clientCode);

        // Daily Activity Report Methods
        Task<List<DailyActivityReportDto>> GetDailyActivityReportAsync(ReportFilterDto filter, string clientCode);
        Task<DailyActivityReportDto> GetDailyActivityByDateAsync(DateTime date, string clientCode);
        Task<DailyActivityReportDto> GetDailyActivityByBranchAsync(DateTime date, int branchId, string clientCode);
        Task<DailyActivityReportDto> GetDailyActivityByCounterAsync(DateTime date, int counterId, string clientCode);
        Task<DailyActivityReportDto> GetDailyActivityByCategoryAsync(DateTime date, int categoryId, string clientCode);

        // Report Summary Methods
        Task<ReportSummaryDto> GetReportSummaryAsync(DateTime date, string clientCode);
        Task<ReportSummaryDto> GetReportSummaryByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode);
        Task<ReportSummaryDto> GetReportSummaryByBranchAsync(DateTime date, int branchId, string clientCode);
        Task<ReportSummaryDto> GetReportSummaryByCounterAsync(DateTime date, int counterId, string clientCode);

        // Stock Tracking Methods
        Task<int> GetCurrentStockAsync(int productId, string clientCode);
        Task<int> GetCurrentStockByBranchAsync(int productId, int branchId, string clientCode);
        Task<int> GetCurrentStockByCounterAsync(int productId, int counterId, string clientCode);
        Task<int> GetCurrentStockByCategoryAsync(int categoryId, string clientCode);
        Task<decimal> GetCurrentStockValueAsync(int productId, string clientCode);
        Task<decimal> GetCurrentStockValueByBranchAsync(int productId, int branchId, string clientCode);
        Task<decimal> GetCurrentStockValueByCounterAsync(int productId, int counterId, string clientCode);
        Task<decimal> GetCurrentStockValueByCategoryAsync(int categoryId, string clientCode);

        // Utility Methods
        Task<bool> ProcessDailyStockBalancesAsync(DateTime date, string clientCode);
        Task<bool> ProcessAllDailyStockBalancesAsync(DateTime startDate, DateTime endDate, string clientCode);
        Task<bool> RecalculateStockBalancesAsync(DateTime startDate, DateTime endDate, string clientCode);

        // RFID Usage Report Methods
        Task<RfidUsageReportDto> GetRfidUsageReportAsync(string clientCode);
        Task<RfidUsageReportDto> GetRfidUsageReportByDateAsync(DateTime date, string clientCode);
        Task<List<RfidUsageDetailDto>> GetUsedRfidTagsAsync(string clientCode);
        Task<List<RfidUsageDetailDto>> GetUnusedRfidTagsAsync(string clientCode);
        Task<List<RfidUsageDetailDto>> GetRfidTagsByStatusAsync(bool isUsed, string clientCode);
        Task<List<RfidUsageByCategoryDto>> GetRfidUsageByCategoryAsync(string clientCode);
        Task<List<RfidUsageByBranchDto>> GetRfidUsageByBranchAsync(string clientCode);
        Task<List<RfidUsageByCounterDto>> GetRfidUsageByCounterAsync(string clientCode);
        Task<RfidUsageByCategoryDto> GetRfidUsageByCategoryIdAsync(int categoryId, string clientCode);
        Task<RfidUsageByBranchDto> GetRfidUsageByBranchIdAsync(int branchId, string clientCode);
        Task<RfidUsageByCounterDto> GetRfidUsageByCounterIdAsync(int counterId, string clientCode);
        Task<int> GetTotalRfidTagsCountAsync(string clientCode);
        Task<int> GetUsedRfidTagsCountAsync(string clientCode);
        Task<int> GetUnusedRfidTagsCountAsync(string clientCode);
        Task<decimal> GetRfidUsagePercentageAsync(string clientCode);
    }
}
