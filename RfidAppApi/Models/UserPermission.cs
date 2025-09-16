using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.Models
{
    public class UserPermission
    {
        [Key]
        public int UserPermissionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string ClientCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Module { get; set; } = string.Empty; // "Product", "RFID", "Invoice", "Reports", "StockTransfer", "StockVerification"

        public bool CanView { get; set; } = false;
        public bool CanCreate { get; set; } = false;
        public bool CanUpdate { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanExport { get; set; } = false;
        public bool CanImport { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
