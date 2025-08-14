using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        [StringLength(100)]
        public string? PageName { get; set; }
        
        public bool CanView { get; set; } = false;
        
        public bool CanEdit { get; set; } = false;
        
        public bool CanDelete { get; set; } = false;
        
        // Navigation property
        public virtual Role Role { get; set; } = null!;
    }
} 