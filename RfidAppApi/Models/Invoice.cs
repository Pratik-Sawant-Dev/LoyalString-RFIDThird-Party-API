using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for storing invoice information in client database
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string? RfidCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        [StringLength(20)]
        public string InvoiceType { get; set; } = "Sale"; // Sale, Return, Exchange

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(15)]
        public string? CustomerPhone { get; set; }

        [StringLength(200)]
        public string? CustomerAddress { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, UPI, etc.

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public DateTime SoldOn { get; set; } = DateTime.UtcNow;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual ProductDetails? Product { get; set; }
    }
}
