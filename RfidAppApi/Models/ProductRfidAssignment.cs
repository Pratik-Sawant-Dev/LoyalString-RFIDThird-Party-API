using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class ProductRfidAssignment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RFIDCode { get; set; } = string.Empty;
        
        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;
        
        public DateTime? UnassignedOn { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ProductDetails Product { get; set; } = null!;
        public virtual Rfid Rfid { get; set; } = null!;
    }
} 