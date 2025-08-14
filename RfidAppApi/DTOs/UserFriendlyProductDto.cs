namespace RfidAppApi.DTOs
{
    /// <summary>
    /// User-friendly DTO for creating products with text-based master data
    /// Users can simply enter text values like "Gold", "Pune", "Counter1" instead of knowing IDs
    /// </summary>
    public class UserFriendlyCreateProductDto
    {
        public string ItemCode { get; set; } = string.Empty;
        
        // User-friendly text inputs instead of IDs
        public string CategoryName { get; set; } = string.Empty; // e.g., "Gold", "Silver", "Diamond"
        public string BranchName { get; set; } = string.Empty;   // e.g., "Pune", "Mumbai", "Delhi"
        public string CounterName { get; set; } = string.Empty;  // e.g., "Counter1", "Counter2", "Main Counter"
        public string ProductName { get; set; } = string.Empty;  // e.g., "Ring", "Necklace", "Bracelet"
        public string DesignName { get; set; } = string.Empty;   // e.g., "Classic", "Modern", "Traditional"
        public string PurityName { get; set; } = string.Empty;   // e.g., "24K", "22K", "18K"
        
        // RFID Code - optional, will be created if not exists
        public string? RfidCode { get; set; } = string.Empty;    // e.g., "RFID001", "RFID002"
        
        // Product details
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
        public string? Status { get; set; } = "Active";
    }

    /// <summary>
    /// User-friendly DTO for updating products
    /// </summary>
    public class UserFriendlyUpdateProductDto
    {
        public string? CategoryName { get; set; }
        public string? BranchName { get; set; }
        public string? CounterName { get; set; }
        public string? ProductName { get; set; }
        public string? DesignName { get; set; }
        public string? PurityName { get; set; }
        
        // RFID Code - optional for updates
        public string? RfidCode { get; set; }
        
        // Product details (all optional for updates)
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

    /// <summary>
    /// Response DTO for user-friendly product operations
    /// </summary>
    public class UserFriendlyProductResponseDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        
        // Master data names (user-friendly)
        public string CategoryName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string CounterName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public string PurityName { get; set; } = string.Empty;
        
        // RFID Code
        public string? RfidCode { get; set; }
        
        // Product details
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
        
        // Master data IDs (for reference)
        public int CategoryId { get; set; }
        public int BranchId { get; set; }
        public int CounterId { get; set; }
        public int ProductId { get; set; }
        public int DesignId { get; set; }
        public int PurityId { get; set; }
    }

    /// <summary>
    /// DTO for bulk product creation with user-friendly inputs
    /// </summary>
    public class BulkCreateProductsDto
    {
        public List<UserFriendlyCreateProductDto> Products { get; set; } = new List<UserFriendlyCreateProductDto>();
    }

    /// <summary>
    /// Response for bulk operations
    /// </summary>
    public class BulkProductResponseDto
    {
        public int TotalProducts { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int Failed { get; set; }
        public List<UserFriendlyProductResponseDto> CreatedProducts { get; set; } = new List<UserFriendlyProductResponseDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for creating invoice
    /// </summary>
    public class CreateInvoiceDto
    {
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public string? InvoiceType { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? SoldOn { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for updating invoice
    /// </summary>
    public class UpdateInvoiceDto
    {
        public decimal? SellingPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for invoice response
    /// </summary>
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime SoldOn { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    /// <summary>
    /// DTO for invoice statistics
    /// </summary>
    public class InvoiceStatisticsDto
    {
        public int TotalInvoices { get; set; }
        public int TodayInvoices { get; set; }
        public int MonthInvoices { get; set; }
        public int YearInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }
    }
}
