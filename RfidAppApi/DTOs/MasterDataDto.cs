using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    // Category Master DTOs
    public class CategoryMasterDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CreateCategoryMasterDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class UpdateCategoryMasterDto
    {
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    // Purity Master DTOs
    public class PurityMasterDto
    {
        public int PurityId { get; set; }
        public string PurityName { get; set; } = string.Empty;
    }

    public class CreatePurityMasterDto
    {
        [Required]
        [StringLength(50)]
        public string PurityName { get; set; } = string.Empty;
    }

    public class UpdatePurityMasterDto
    {
        [Required]
        public int PurityId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PurityName { get; set; } = string.Empty;
    }

    // Design Master DTOs
    public class DesignMasterDto
    {
        public int DesignId { get; set; }
        public string DesignName { get; set; } = string.Empty;
    }

    public class CreateDesignMasterDto
    {
        [Required]
        [StringLength(100)]
        public string DesignName { get; set; } = string.Empty;
    }

    public class UpdateDesignMasterDto
    {
        [Required]
        public int DesignId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DesignName { get; set; } = string.Empty;
    }

    // Box Master DTOs
    public class BoxMasterDto
    {
        public int BoxId { get; set; }
        public string BoxName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BoxType { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class CreateBoxMasterDto
    {
        [Required]
        [StringLength(100)]
        public string BoxName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? BoxType { get; set; }
        
        [StringLength(50)]
        public string? Size { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Material { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBoxMasterDto
    {
        [Required]
        public int BoxId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BoxName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? BoxType { get; set; }
        
        [StringLength(50)]
        public string? Size { get; set; }
        
        [StringLength(50)]
        public string? Color { get; set; }
        
        [StringLength(50)]
        public string? Material { get; set; }
        
        public bool IsActive { get; set; }
    }

    // Counter Master DTOs
    public class CounterMasterDto
    {
        public int CounterId { get; set; }
        public string CounterName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
    }

    public class CreateCounterMasterDto
    {
        [Required]
        [StringLength(100)]
        public string CounterName { get; set; } = string.Empty;
        
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
    }

    public class UpdateCounterMasterDto
    {
        [Required]
        public int CounterId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CounterName { get; set; } = string.Empty;
        
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
    }

    // Branch Master DTOs
    public class BranchMasterDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public int CounterCount { get; set; }
    }

    public class CreateBranchMasterDto
    {
        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
    }

    public class UpdateBranchMasterDto
    {
        [Required]
        public int BranchId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string ClientCode { get; set; } = string.Empty;
    }

    // Product Master DTOs
    public class ProductMasterDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }

    public class CreateProductMasterDto
    {
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
    }

    public class UpdateProductMasterDto
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
    }

    // Master Data Summary DTOs
    public class MasterDataSummaryDto
    {
        public int TotalCategories { get; set; }
        public int TotalPurities { get; set; }
        public int TotalDesigns { get; set; }
        public int TotalBoxes { get; set; }
        public int TotalCounters { get; set; }
        public int TotalBranches { get; set; }
        public int TotalProducts { get; set; }
    }

    public class MasterDataCountsDto
    {
        public string EntityName { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
