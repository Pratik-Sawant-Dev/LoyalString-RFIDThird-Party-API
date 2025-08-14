using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class Rfid
    {
        [Key]
        [StringLength(50)]
        public string RFIDCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EPCValue { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
} 