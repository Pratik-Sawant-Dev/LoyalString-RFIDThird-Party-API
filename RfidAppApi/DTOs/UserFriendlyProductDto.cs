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
        // Support both field names for flexibility
        public string? RfidCode { get; set; } = string.Empty;    // e.g., "RFID001", "RFID002"
        public string? RFIDNumber { get; set; } = string.Empty;  // Alternative field name for compatibility
        
        // Computed property to get RFID code from either field
        public string? GetRfidCode() => !string.IsNullOrWhiteSpace(RfidCode) ? RfidCode : RFIDNumber;
        
        // Product details
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
        public string? BoxDetails { get; set; } // e.g., "Box A", "Box B", "Premium Box"
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
        
        // Custom Fields - Dictionary for flexible field names and values
        public Dictionary<string, object>? CustomFields { get; set; }
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
        
        // Box Details - optional for updates
        public string? BoxDetails { get; set; }
        
        // RFID Code - optional for updates
        public string? RfidCode { get; set; }
        
        // Product details (all optional for updates)
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
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
        
        // Custom Fields
        public Dictionary<string, object>? CustomFields { get; set; }
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
        
        // Box Details
        public string? BoxDetails { get; set; } = string.Empty;
        
        // RFID Code
        public string? RfidCode { get; set; }
        
        // Product details
        public float? GrossWeight { get; set; }
        public float? StoneWeight { get; set; }
        public float? DiamondHeight { get; set; }
        public float? NetWeight { get; set; }
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
        
        // Custom Fields
        public List<CustomFieldDto>? CustomFields { get; set; }
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
        public bool IsGstApplied { get; set; } = false; // true = Pakka Bill, false = Kaccha Bill
        public decimal GstPercentage { get; set; } = 3.00m; // Default 3% GST for jewelry
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
        public bool? IsGstApplied { get; set; }
        public decimal? GstPercentage { get; set; }
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
        public bool IsGstApplied { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal AmountBeforeGst { get; set; }
        public decimal TotalAmountWithGst { get; set; }
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

    /// <summary>
    /// DTO for individual payment method in an invoice
    /// </summary>
    public class PaymentMethodDto
    {
        public string PaymentMethod { get; set; } = string.Empty; // Cash, UPI, Card, Online, etc.
        public decimal Amount { get; set; }
        public string? PaymentReference { get; set; } // Transaction ID, UPI reference, etc.
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Enhanced DTO for creating invoice with multiple payment methods
    /// </summary>
    public class CreateInvoiceWithMultiplePaymentsDto
    {
        public int ProductId { get; set; }
        public string? RfidCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public bool IsGstApplied { get; set; } = false; // true = Pakka Bill, false = Kaccha Bill
        public decimal GstPercentage { get; set; } = 3.00m; // Default 3% GST for jewelry
        public string? InvoiceType { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public List<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();
        public DateTime? SoldOn { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for creating invoice by item code
    /// </summary>
    public class CreateInvoiceByItemCodeDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public bool IsGstApplied { get; set; } = false; // true = Pakka Bill, false = Kaccha Bill
        public decimal GstPercentage { get; set; } = 3.00m; // Default 3% GST for jewelry
        public string? InvoiceType { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public List<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();
        public DateTime? SoldOn { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Enhanced DTO for invoice response with multiple payment methods
    /// </summary>
    public class InvoiceWithPaymentsResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public bool IsGstApplied { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal AmountBeforeGst { get; set; }
        public decimal TotalAmountWithGst { get; set; }
        public string InvoiceType { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public List<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();
        public DateTime SoldOn { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
