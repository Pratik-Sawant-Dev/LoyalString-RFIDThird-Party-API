using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for storing individual payment methods for an invoice
    /// </summary>
    public class InvoicePayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, UPI, Card, Online, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaymentReference { get; set; } // Transaction ID, UPI reference, etc.

        [StringLength(200)]
        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; } = null!;
    }
}
