using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidAppApi.Models
{
    public class ProductDetails
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
        
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        public int CounterId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        // Master References
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int DesignId { get; set; }
        
        [Required]
        public int PurityId { get; set; }

        // Weight Details
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }

        // Product Details
        [StringLength(100)]
        public string? BoxDetails { get; set; }
        public int? Size { get; set; }

        // Pricing Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal? StoneAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiamondAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? HallmarkAmount { get; set; }

        // Making Charges
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MakingPerGram { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MakingPercentage { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MakingFixedAmount { get; set; }

        // Final Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Mrp { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual CategoryMaster Category { get; set; } = null!;
        public virtual ProductMaster Product { get; set; } = null!;
        public virtual DesignMaster Design { get; set; } = null!;
        public virtual PurityMaster Purity { get; set; } = null!;
        public virtual BranchMaster Branch { get; set; } = null!;
        public virtual CounterMaster Counter { get; set; } = null!;
    }
} 