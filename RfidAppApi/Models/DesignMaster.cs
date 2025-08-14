using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class DesignMaster
    {
        [Key]
        public int DesignId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DesignName { get; set; } = string.Empty;
    }
} 