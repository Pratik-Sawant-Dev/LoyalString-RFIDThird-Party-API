using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class CategoryMaster
    {
        [Key]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }
} 