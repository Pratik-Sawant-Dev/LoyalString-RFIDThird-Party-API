using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    /// <summary>
    /// Master table for jewelry box types and specifications
    /// Examples: "Premium Wooden Box", "Velvet Case", "Gift Box", "Travel Case"
    /// </summary>
    public class BoxMaster
    {
        [Key]
        public int BoxId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BoxName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? BoxType { get; set; } // e.g., "Wooden", "Velvet", "Plastic", "Gift"
        
        [StringLength(50)]
        public string? Size { get; set; } // e.g., "Small", "Medium", "Large", "Extra Large"
        
        [StringLength(50)]
        public string? Color { get; set; } // e.g., "Brown", "Black", "Red", "Blue"
        
        [StringLength(50)]
        public string? Material { get; set; } // e.g., "Wood", "Velvet", "Leather", "Plastic"
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedOn { get; set; }
    }
}
