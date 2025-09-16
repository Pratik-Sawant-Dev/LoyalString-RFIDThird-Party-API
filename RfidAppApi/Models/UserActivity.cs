using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class UserActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty; // "Product", "RFID", "Invoice", "Login", "Logout"

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // "Create", "Update", "Delete", "View", "Login", "Logout"

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? TableName { get; set; }

        public int? RecordId { get; set; }

        [StringLength(2000)]
        public string? OldValues { get; set; } // JSON string of old values

        [StringLength(2000)]
        public string? NewValues { get; set; } // JSON string of new values

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
