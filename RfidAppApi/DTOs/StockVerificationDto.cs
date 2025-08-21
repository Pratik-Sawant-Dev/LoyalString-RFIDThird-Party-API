using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    /// <summary>
    /// DTO for creating a new stock verification session
    /// </summary>
    public class CreateStockVerificationDto
    {
        [Required]
        public string VerificationSessionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime VerificationDate { get; set; }

        [Required]
        public TimeSpan VerificationTime { get; set; }

        [Required]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        public string CounterName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public string? VerifiedBy { get; set; }

        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for submitting scanned items for verification
    /// </summary>
    public class SubmitStockVerificationDto
    {
        [Required]
        public int StockVerificationId { get; set; }

        [Required]
        public List<string> MatchedItemCodes { get; set; } = new List<string>();

        [Required]
        public List<string> UnmatchedItemCodes { get; set; } = new List<string>();

        public string? ScannedBy { get; set; }

        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for stock verification response
    /// </summary>
    public class StockVerificationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StockVerificationDto? Data { get; set; }
    }

    /// <summary>
    /// DTO for stock verification details
    /// </summary>
    public class StockVerificationDto
    {
        public int Id { get; set; }
        public string VerificationSessionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime VerificationDate { get; set; }
        public TimeSpan VerificationTime { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalItemsScanned { get; set; }
        public int MatchedItemsCount { get; set; }
        public int UnmatchedItemsCount { get; set; }
        public int MissingItemsCount { get; set; }
        public decimal? TotalMatchedValue { get; set; }
        public decimal? TotalUnmatchedValue { get; set; }
        public decimal? TotalMissingValue { get; set; }
        public string? VerifiedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public List<StockVerificationDetailDto> VerificationDetails { get; set; } = new List<StockVerificationDetailDto>();
    }

    /// <summary>
    /// DTO for individual verification detail
    /// </summary>
    public class StockVerificationDetailDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime ScannedAt { get; set; }
        public TimeSpan ScannedTime { get; set; }
        public string? ScannedBy { get; set; }
        public string? Remarks { get; set; }
        
        // Product details for matched items
        public ProductDetailsDto? ProductDetails { get; set; }
    }

    /// <summary>
    /// DTO for stock verification summary
    /// </summary>
    public class StockVerificationSummaryDto
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int InProgressSessions { get; set; }
        public int TotalItemsVerified { get; set; }
        public int TotalMatchedItems { get; set; }
        public int TotalUnmatchedItems { get; set; }
        public int TotalMissingItems { get; set; }
        public decimal? TotalMatchedValue { get; set; }
        public decimal? TotalUnmatchedValue { get; set; }
        public decimal? TotalMissingValue { get; set; }
        public List<StockVerificationDto> RecentSessions { get; set; } = new List<StockVerificationDto>();
    }

    /// <summary>
    /// DTO for stock verification report filter
    /// </summary>
    public class StockVerificationReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BranchName { get; set; }
        public string? CounterName { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
        public string? VerifiedBy { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// DTO for stock verification report response
    /// </summary>
    public class StockVerificationReportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<StockVerificationDto> Data { get; set; } = new List<StockVerificationDto>();
        public StockVerificationSummaryDto Summary { get; set; } = new StockVerificationSummaryDto();
    }

    /// <summary>
    /// DTO for date-wise stock verification report
    /// </summary>
    public class DateWiseStockVerificationReportDto
    {
        public DateTime Date { get; set; }
        public int TotalSessions { get; set; }
        public int TotalItemsScanned { get; set; }
        public int TotalMatchedItems { get; set; }
        public int TotalUnmatchedItems { get; set; }
        public int TotalMissingItems { get; set; }
        public decimal? TotalMatchedValue { get; set; }
        public decimal? TotalUnmatchedValue { get; set; }
        public decimal? TotalMissingValue { get; set; }
        public List<StockVerificationDto> Sessions { get; set; } = new List<StockVerificationDto>();
    }
}
