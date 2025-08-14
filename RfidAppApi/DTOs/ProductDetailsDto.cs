namespace RfidAppApi.DTOs
{
    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public int CounterId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public int DesignId { get; set; }
        public int PurityId { get; set; }
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
        public string? BoxDetails { get; set; }
        public int? Size { get; set; }
        public decimal? StoneAmount { get; set; }
        public decimal? DiamondAmount { get; set; }
        public decimal? HallmarkAmount { get; set; }
        public decimal? MakingPerGram { get; set; }
        public decimal? MakingPercentage { get; set; }
        public decimal? MakingFixedAmount { get; set; }
        public decimal? Mrp { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedOn { get; set; }
        
        // Navigation properties
        public string? CategoryName { get; set; }
        public string? ProductName { get; set; }
        public string? DesignName { get; set; }
        public string? PurityName { get; set; }
        public string? BranchName { get; set; }
        public string? CounterName { get; set; }
    }

    public class CreateProductDetailsDto
    {
        public string ClientCode { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public int CounterId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public int DesignId { get; set; }
        public int PurityId { get; set; }
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
        public string? BoxDetails { get; set; }
        public int? Size { get; set; }
        public decimal? StoneAmount { get; set; }
        public decimal? DiamondAmount { get; set; }
        public decimal? HallmarkAmount { get; set; }
        public decimal? MakingPerGram { get; set; }
        public decimal? MakingPercentage { get; set; }
        public decimal? MakingFixedAmount { get; set; }
        public decimal? Mrp { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateProductDetailsDto
    {
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public int? DesignId { get; set; }
        public int? PurityId { get; set; }
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
        public string? BoxDetails { get; set; }
        public int? Size { get; set; }
        public decimal? StoneAmount { get; set; }
        public decimal? DiamondAmount { get; set; }
        public decimal? HallmarkAmount { get; set; }
        public decimal? MakingPerGram { get; set; }
        public decimal? MakingPercentage { get; set; }
        public decimal? MakingFixedAmount { get; set; }
        public decimal? Mrp { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
    }
} 