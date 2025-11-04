using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        // Authentication
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // Personal Information
        [StringLength(150)]
        public string? FullName { get; set; }
        
        [StringLength(20)]
        public string? MobileNumber { get; set; }
        
        [StringLength(50)]
        public string? FaxNumber { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(255)]
        public string? Address { get; set; }
        
        [Required]
        [StringLength(150)]
        public string OrganisationName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ShowroomType { get; set; }

        // Organization Details
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        // Database Connection Details
        [StringLength(255)]
        public string? DatabaseName { get; set; }
        
        [StringLength(500)]
        public string? ConnectionString { get; set; }

        // Admin-User Hierarchy
        public bool IsAdmin { get; set; } = false;
        public int? AdminUserId { get; set; } // Reference to admin user (null for main admin)
        
        [StringLength(50)]
        public string UserType { get; set; } = "User"; // "MainAdmin", "Admin", "User"

        // Branch and Counter assignment for sub-users
        public int? BranchId { get; set; } // Reference to branch (null for main admin and admin users)
        public int? CounterId { get; set; } // Reference to counter (null for main admin and admin users)

        // Status
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }

        // Password Reset
        [StringLength(500)]
        public string? PasswordResetToken { get; set; }
        
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Navigation properties
        public virtual User? AdminUser { get; set; }
    }
} 