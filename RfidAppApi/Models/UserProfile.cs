using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    /// <summary>
    /// User profile model for storing profile images and extended address information
    /// </summary>
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Profile Image
        [StringLength(500)]
        public string? ProfileImagePath { get; set; }

        [StringLength(255)]
        public string? ProfileImageFileName { get; set; }

        [StringLength(100)]
        public string? ProfileImageContentType { get; set; }

        public long? ProfileImageFileSize { get; set; }

        // Extended Address Information
        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(255)]
        public string? AddressLine1 { get; set; }

        [StringLength(255)]
        public string? AddressLine2 { get; set; }

        [StringLength(100)]
        public string? Landmark { get; set; }

        // Additional Profile Information
        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(50)]
        public string? AlternatePhone { get; set; }

        // Profile Completion Status
        public bool IsProfileComplete { get; set; } = false;

        public DateTime? ProfileCompletedOn { get; set; }

        // Timestamps
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}

