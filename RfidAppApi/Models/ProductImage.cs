using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Model for storing product images in client database
    /// </summary>
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [StringLength(100)]
        public string? ImageType { get; set; } // "Primary", "Secondary", "Thumbnail"

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual ProductDetails? Product { get; set; }
    }
}
