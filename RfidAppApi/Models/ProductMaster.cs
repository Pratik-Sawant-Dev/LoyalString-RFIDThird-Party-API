using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class ProductMaster
    {
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
    }
} 