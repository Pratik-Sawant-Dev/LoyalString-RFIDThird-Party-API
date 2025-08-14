using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for tracking all stock movements (additions, sales, returns, transfers)
    /// </summary>
    public class StockMovement
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
        [StringLength(20)]
        public string MovementType { get; set; } = string.Empty; // Addition, Sale, Return, Transfer, Adjustment

        [Required]
        public int Quantity { get; set; } = 1; // Always 1 for jewelry items

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; } // MRP for additions, Selling price for sales

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int CounterId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Invoice number for sales, etc.

        [StringLength(50)]
        public string? ReferenceType { get; set; } // Invoice, Transfer, Adjustment

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;

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
