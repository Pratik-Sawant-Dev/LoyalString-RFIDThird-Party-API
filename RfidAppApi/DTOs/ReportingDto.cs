namespace RfidAppApi.DTOs
{
    /// <summary>
    /// DTO for stock movement tracking
    /// </summary>
    public class StockMovementDto
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? ReferenceType { get; set; }
        public string? Remarks { get; set; }
        public DateTime MovementDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// DTO for daily stock balance
    /// </summary>
    public class DailyStockBalanceDto
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime BalanceDate { get; set; }
        public int OpeningQuantity { get; set; }
        public int ClosingQuantity { get; set; }
        public int AddedQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public int ReturnedQuantity { get; set; }
        public int TransferredInQuantity { get; set; }
        public int TransferredOutQuantity { get; set; }
        public decimal? OpeningValue { get; set; }
        public decimal? ClosingValue { get; set; }
        public decimal? AddedValue { get; set; }
        public decimal? SoldValue { get; set; }
        public decimal? ReturnedValue { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// DTO for sales report
    /// </summary>
    public class SalesReportDto
    {
        public DateTime Date { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalItemsSold { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal NetSalesAmount { get; set; }
        public int TotalInvoices { get; set; }
        public decimal AverageTicketValue { get; set; }
    }

    /// <summary>
    /// DTO for stock summary report
    /// </summary>
    public class StockSummaryReportDto
    {
        public DateTime Date { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int SoldProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public decimal TotalSalesValue { get; set; }
    }

    /// <summary>
    /// DTO for daily activity report
    /// </summary>
    public class DailyActivityReportDto
    {
        public DateTime Date { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int OpeningStock { get; set; }
        public int AddedStock { get; set; }
        public int SoldStock { get; set; }
        public int ReturnedStock { get; set; }
        public int TransferredInStock { get; set; }
        public int TransferredOutStock { get; set; }
        public int ClosingStock { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal AddedValue { get; set; }
        public decimal SoldValue { get; set; }
        public decimal ReturnedValue { get; set; }
        public decimal TransferredInValue { get; set; }
        public decimal TransferredOutValue { get; set; }
        public decimal ClosingValue { get; set; }
    }

    /// <summary>
    /// DTO for report filters
    /// </summary>
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BranchId { get; set; }
        public int? CounterId { get; set; }
        public int? CategoryId { get; set; }
        public string? MovementType { get; set; }
        public string? RfidCode { get; set; }
        public string? ItemCode { get; set; }
    }

    /// <summary>
    /// DTO for stock movement creation
    /// </summary>
    public class CreateStockMovementDto
    {
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal? UnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ReferenceType { get; set; }
        public string? Remarks { get; set; }
        public DateTime? MovementDate { get; set; }
    }

    /// <summary>
    /// DTO for bulk stock movement creation
    /// </summary>
    public class BulkStockMovementDto
    {
        public List<CreateStockMovementDto> Movements { get; set; } = new List<CreateStockMovementDto>();
        public DateTime? MovementDate { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for report summary
    /// </summary>
    public class ReportSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalProducts { get; set; }
        public int TotalAdded { get; set; }
        public int TotalSold { get; set; }
        public int TotalReturned { get; set; }
        public decimal TotalAddedValue { get; set; }
        public decimal TotalSoldValue { get; set; }
        public decimal TotalReturnedValue { get; set; }
        public int TotalInvoices { get; set; }
        public decimal NetSalesAmount { get; set; }
    }

    /// <summary>
    /// DTO for RFID usage report
    /// </summary>
    public class RfidUsageReportDto
    {
        public string ClientCode { get; set; } = string.Empty;
        public int TotalRfidTags { get; set; }
        public int UsedRfidTags { get; set; }
        public int UnusedRfidTags { get; set; }
        public decimal UsagePercentage { get; set; }
        public decimal UnusedPercentage { get; set; }
        public DateTime ReportDate { get; set; }
        public List<RfidUsageDetailDto> UsedRfidDetails { get; set; } = new List<RfidUsageDetailDto>();
        public List<RfidUsageDetailDto> UnusedRfidDetails { get; set; } = new List<RfidUsageDetailDto>();
    }

    /// <summary>
    /// DTO for RFID usage detail
    /// </summary>
    public class RfidUsageDetailDto
    {
        public string RfidCode { get; set; } = string.Empty;
        public string EpcValue { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public int? ProductId { get; set; }
        public string? ItemCode { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryName { get; set; }
        public string? BranchName { get; set; }
        public string? CounterName { get; set; }
        public DateTime? AssignedOn { get; set; }
        public DateTime? UnassignedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    /// <summary>
    /// DTO for RFID usage summary by category
    /// </summary>
    public class RfidUsageByCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalRfidTags { get; set; }
        public int UsedRfidTags { get; set; }
        public int UnusedRfidTags { get; set; }
        public decimal UsagePercentage { get; set; }
    }

    /// <summary>
    /// DTO for RFID usage summary by branch
    /// </summary>
    public class RfidUsageByBranchDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalRfidTags { get; set; }
        public int UsedRfidTags { get; set; }
        public int UnusedRfidTags { get; set; }
        public decimal UsagePercentage { get; set; }
    }

    /// <summary>
    /// DTO for RFID usage summary by counter
    /// </summary>
    public class RfidUsageByCounterDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalRfidTags { get; set; }
        public int UsedRfidTags { get; set; }
        public int UnusedRfidTags { get; set; }
        public decimal UsagePercentage { get; set; }
    }
}
