using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for tracking daily opening and closing stock balances
    /// </summary>
    public class DailyStockBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string? RfidCode { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int CounterId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public DateTime BalanceDate { get; set; }

        [Required]
        public int OpeningQuantity { get; set; } = 0;

        [Required]
        public int ClosingQuantity { get; set; } = 0;

        [Required]
        public int AddedQuantity { get; set; } = 0;

        [Required]
        public int SoldQuantity { get; set; } = 0;

        [Required]
        public int ReturnedQuantity { get; set; } = 0;

        [Required]
        public int TransferredInQuantity { get; set; } = 0;

        [Required]
        public int TransferredOutQuantity { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OpeningValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClosingValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AddedValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoldValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReturnedValue { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual ProductDetails? Product { get; set; }

        [ForeignKey("BranchId")]
        public virtual BranchMaster? Branch { get; set; }

        [ForeignKey("CounterId")]
        public virtual CounterMaster? Counter { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CategoryMaster? Category { get; set; }
    }
}
