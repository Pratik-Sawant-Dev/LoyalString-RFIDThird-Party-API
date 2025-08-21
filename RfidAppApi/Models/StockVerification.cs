using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for tracking stock verification sessions and results
    /// </summary>
    public class StockVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string VerificationSessionName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime VerificationDate { get; set; }

        [Required]
        public TimeSpan VerificationTime { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int CounterId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int TotalItemsScanned { get; set; } = 0;

        [Required]
        public int MatchedItemsCount { get; set; } = 0;

        [Required]
        public int UnmatchedItemsCount { get; set; } = 0;

        [Required]
        public int MissingItemsCount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalMatchedValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalUnmatchedValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalMissingValue { get; set; }

        [StringLength(50)]
        public string? VerifiedBy { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("BranchId")]
        public virtual BranchMaster Branch { get; set; } = null!;

        [ForeignKey("CounterId")]
        public virtual CounterMaster Counter { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual CategoryMaster Category { get; set; } = null!;

        // Collection of verification details
        public virtual ICollection<StockVerificationDetail> VerificationDetails { get; set; } = new List<StockVerificationDetail>();
    }

    /// <summary>
    /// Model for individual items in a stock verification session
    /// </summary>
    public class StockVerificationDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StockVerificationId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RfidCode { get; set; }

        [Required]
        [StringLength(20)]
        public string VerificationStatus { get; set; } = string.Empty; // Matched, Unmatched, Missing

        [Required]
        public DateTime ScannedAt { get; set; }

        [Required]
        public TimeSpan ScannedTime { get; set; }

        [StringLength(100)]
        public string? ScannedBy { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("StockVerificationId")]
        public virtual StockVerification StockVerification { get; set; } = null!;
    }
}
