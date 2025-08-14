using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class BranchMaster
    {
        [Key]
        public int BranchId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
    }
} 