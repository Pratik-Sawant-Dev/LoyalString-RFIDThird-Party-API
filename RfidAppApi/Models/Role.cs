using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Description { get; set; }
    }
} 