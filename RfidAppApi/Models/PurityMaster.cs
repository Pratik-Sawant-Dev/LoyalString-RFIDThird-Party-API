using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class PurityMaster
    {
        [Key]
        public int PurityId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PurityName { get; set; } = string.Empty;
    }
} 