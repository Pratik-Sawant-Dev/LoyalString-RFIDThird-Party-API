using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class CounterMaster
    {
        [Key]
        public int CounterId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CounterName { get; set; } = string.Empty;
        
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
        
        // Navigation property
        public virtual BranchMaster Branch { get; set; } = null!;
    }
} 