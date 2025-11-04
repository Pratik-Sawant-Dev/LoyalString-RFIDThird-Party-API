using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    public class ProductCustomField
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductDetailsId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FieldName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? FieldValue { get; set; }
        
        [StringLength(50)]
        public string FieldType { get; set; } = "Text"; // Text, Number, Decimal, Date, Boolean
        
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("ProductDetailsId")]
        public virtual ProductDetails ProductDetails { get; set; } = null!;
    }
}

