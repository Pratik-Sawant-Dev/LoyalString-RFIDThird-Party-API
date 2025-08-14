using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
} 