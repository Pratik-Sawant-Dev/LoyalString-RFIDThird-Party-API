using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    #region Request DTOs

    /// <summary>
    /// DTO for creating a new stock transfer
    /// </summary>
    public class CreateStockTransferDto
    {
        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string? RfidCode { get; set; }

        [Required]
        [StringLength(20)]
        public string TransferType { get; set; } = string.Empty; // Branch, Counter, Box, Mixed

        // Source Location
        [Required]
        public int SourceBranchId { get; set; }

        [Required]
        public int SourceCounterId { get; set; }

        public int? SourceBoxId { get; set; }

        // Destination Location
        [Required]
        public int DestinationBranchId { get; set; }

        [Required]
        public int DestinationCounterId { get; set; }

        public int? DestinationBoxId { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for bulk stock transfers
    /// </summary>
    public class BulkStockTransferDto
    {
        [Required]
        public List<CreateStockTransferDto> Transfers { get; set; } = new();

        [StringLength(500)]
        public string? CommonReason { get; set; }

        [StringLength(500)]
        public string? CommonRemarks { get; set; }
    }

    /// <summary>
    /// DTO for updating transfer status
    /// </summary>
    public class UpdateTransferStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Pending, InTransit, Completed, Cancelled, Rejected

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(100)]
        public string? ActionBy { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; } // Required if status is Rejected
    }

    /// <summary>
    /// DTO for transfer approval
    /// </summary>
    public class ApproveTransferDto
    {
        [Required]
        [StringLength(100)]
        public string ApprovedBy { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for transfer rejection
    /// </summary>
    public class RejectTransferDto
    {
        [Required]
        [StringLength(100)]
        public string RejectedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RejectionReason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for transfer filters
    /// </summary>
    public class TransferFilterDto
    {
        public int? ProductId { get; set; }
        public string? RfidCode { get; set; }
        public string? TransferType { get; set; }
        public int? SourceBranchId { get; set; }
        public int? SourceCounterId { get; set; }
        public int? DestinationBranchId { get; set; }
        public int? DestinationCounterId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// DTO for stock transfer response
    /// </summary>
    public class StockTransferResponseDto
    {
        public int Id { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public string TransferType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Reason { get; set; }
        public string? Remarks { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedOn { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Source Location Details
        public LocationInfo SourceLocation { get; set; } = new();
        public LocationInfo DestinationLocation { get; set; } = new();

        // Product Details
        public ProductInfo Product { get; set; } = new();
    }

    /// <summary>
    /// DTO for location information
    /// </summary>
    public class LocationInfo
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int? BoxId { get; set; }
        public string? BoxName { get; set; }
    }

    /// <summary>
    /// DTO for product information in transfer
    /// </summary>
    public class ProductInfo
    {
        public string ItemCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public string PurityName { get; set; } = string.Empty;
        public float? GrossWeight { get; set; }
        public float? NetWeight { get; set; }
        public decimal? Mrp { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for bulk transfer response
    /// </summary>
    public class BulkTransferResponseDto
    {
        public int TotalTransfers { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int Failed { get; set; }
        public List<StockTransferResponseDto> CreatedTransfers { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO for transfer summary
    /// </summary>
    public class TransferSummaryDto
    {
        public int TotalTransfers { get; set; }
        public int PendingTransfers { get; set; }
        public int InTransitTransfers { get; set; }
        public int CompletedTransfers { get; set; }
        public int CancelledTransfers { get; set; }
        public int RejectedTransfers { get; set; }
        public decimal TotalValue { get; set; }
        public List<TransferTypeSummary> TransferTypeSummary { get; set; } = new();
        public List<BranchTransferSummary> BranchTransferSummary { get; set; } = new();
    }

    /// <summary>
    /// DTO for transfer type summary
    /// </summary>
    public class TransferTypeSummary
    {
        public string TransferType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// DTO for branch transfer summary
    /// </summary>
    public class BranchTransferSummary
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int IncomingTransfers { get; set; }
        public int OutgoingTransfers { get; set; }
        public decimal IncomingValue { get; set; }
        public decimal OutgoingValue { get; set; }
    }

    #endregion

    #region Enums

    /// <summary>
    /// Transfer types
    /// </summary>
    public static class TransferTypes
    {
        public const string Branch = "Branch";
        public const string Counter = "Counter";
        public const string Box = "Box";
        public const string Mixed = "Mixed";
    }

    /// <summary>
    /// Transfer statuses
    /// </summary>
    public static class TransferStatuses
    {
        public const string Pending = "Pending";
        public const string InTransit = "InTransit";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";
    }

    #endregion
}
