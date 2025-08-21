using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for tracking stock transfers between branches, counters, and boxes
    /// </summary>
    public class StockTransfer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TransferNumber { get; set; } = string.Empty; // Auto-generated unique transfer number

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

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InTransit, Completed, Cancelled, Rejected

        [Required]
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        [StringLength(100)]
        public string? RejectedBy { get; set; }

        public DateTime? RejectedOn { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual ProductDetails? Product { get; set; }

        [ForeignKey("SourceBranchId")]
        public virtual BranchMaster? SourceBranch { get; set; }

        [ForeignKey("SourceCounterId")]
        public virtual CounterMaster? SourceCounter { get; set; }

        [ForeignKey("SourceBoxId")]
        public virtual BoxMaster? SourceBox { get; set; }

        [ForeignKey("DestinationBranchId")]
        public virtual BranchMaster? DestinationBranch { get; set; }

        [ForeignKey("DestinationCounterId")]
        public virtual CounterMaster? DestinationCounter { get; set; }

        [ForeignKey("DestinationBoxId")]
        public virtual BoxMaster? DestinationBox { get; set; }
    }
}
