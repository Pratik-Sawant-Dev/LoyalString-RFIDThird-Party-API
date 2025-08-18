using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    public class RfidDto
    {
        public string RFIDCode { get; set; } = string.Empty;
        public string EPCValue { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateRfidDto
    {
        public string RFIDCode { get; set; } = string.Empty;
        public string EPCValue { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
    }

    public class UpdateRfidDto
    {
        public string? EPCValue { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for used RFID tag analysis - includes count and detailed RFID information
    /// </summary>
    public class UsedRfidAnalysisDto
    {
        /// <summary>
        /// Total count of used RFID tags
        /// </summary>
        public int TotalUsedCount { get; set; }

        /// <summary>
        /// List of used RFID tags with their details
        /// </summary>
        public List<UsedRfidDetailDto> UsedRfids { get; set; } = new List<UsedRfidDetailDto>();

        /// <summary>
        /// Analysis summary
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for individual used RFID tag details
    /// </summary>
    public class UsedRfidDetailDto
    {
        /// <summary>
        /// RFID tag code
        /// </summary>
        public string RFIDCode { get; set; } = string.Empty;

        /// <summary>
        /// EPC value of the RFID tag
        /// </summary>
        public string EPCValue { get; set; } = string.Empty;

        /// <summary>
        /// Product ID this RFID is assigned to
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// When the RFID was assigned to the product
        /// </summary>
        public DateTime AssignedOn { get; set; }

        /// <summary>
        /// Product details (if available)
        /// </summary>
        public string? ProductInfo { get; set; }
    }

    /// <summary>
    /// DTO for unused RFID tag analysis - includes count and detailed RFID information
    /// </summary>
    public class UnusedRfidAnalysisDto
    {
        /// <summary>
        /// Total count of unused RFID tags
        /// </summary>
        public int TotalUnusedCount { get; set; }

        /// <summary>
        /// List of unused RFID tags with their details
        /// </summary>
        public List<UnusedRfidDetailDto> UnusedRfids { get; set; } = new List<UnusedRfidDetailDto>();

        /// <summary>
        /// Analysis summary
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for individual unused RFID tag details
    /// </summary>
    public class UnusedRfidDetailDto
    {
        /// <summary>
        /// RFID tag code
        /// </summary>
        public string RFIDCode { get; set; } = string.Empty;

        /// <summary>
        /// EPC value of the RFID tag
        /// </summary>
        public string EPCValue { get; set; } = string.Empty;

        /// <summary>
        /// When the RFID was created
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Whether the RFID is active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for RFID scan request - user provides EPC value to scan
    /// </summary>
    public class RfidScanRequestDto
    {
        /// <summary>
        /// Single EPC value to scan for (for backward compatibility)
        /// </summary>
        public string? EpcValue { get; set; }

        /// <summary>
        /// Multiple EPC values to scan for (new feature)
        /// </summary>
        public List<string>? EpcValues { get; set; }
    }

    /// <summary>
    /// DTO for RFID scan response - returns all products associated with scanned EPC values
    /// </summary>
    public class RfidScanResponseDto
    {
        /// <summary>
        /// Whether the scan was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the scan result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Total number of products found
        /// </summary>
        public int TotalProductsFound { get; set; }

        /// <summary>
        /// List of scanned products grouped by EPC value
        /// </summary>
        public List<EpcScanResultDto> ScanResults { get; set; } = new List<EpcScanResultDto>();

        /// <summary>
        /// EPC values that were scanned but found no products
        /// </summary>
        public List<string> UnmatchedEpcValues { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for individual EPC scan result
    /// </summary>
    public class EpcScanResultDto
    {
        /// <summary>
        /// The EPC value that was scanned
        /// </summary>
        public string EpcValue { get; set; } = string.Empty;

        /// <summary>
        /// RFID code associated with this EPC value
        /// </summary>
        public string RfidCode { get; set; } = string.Empty;

        /// <summary>
        /// Number of products found for this EPC value
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// List of products associated with this EPC value
        /// </summary>
        public List<ScannedProductDto> Products { get; set; } = new List<ScannedProductDto>();
    }

    /// <summary>
    /// DTO for individual product found during RFID scan
    /// </summary>
    public class ScannedProductDto
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product item code
        /// </summary>
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>
        /// RFID code assigned to this product
        /// </summary>
        public string RFIDCode { get; set; } = string.Empty;

        /// <summary>
        /// EPC value of the RFID tag
        /// </summary>
        public string EPCValue { get; set; } = string.Empty;

        /// <summary>
        /// When the RFID was assigned to this product
        /// </summary>
        public DateTime AssignedOn { get; set; }

        /// <summary>
        /// Product status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Product category name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Design name
        /// </summary>
        public string DesignName { get; set; } = string.Empty;

        /// <summary>
        /// Purity name
        /// </summary>
        public string PurityName { get; set; } = string.Empty;

        /// <summary>
        /// Branch name where product is located
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Counter name where product is located
        /// </summary>
        public string CounterName { get; set; } = string.Empty;

        /// <summary>
        /// Gross weight
        /// </summary>
        public float? GrossWeight { get; set; }

        /// <summary>
        /// Net weight
        /// </summary>
        public float? NetWeight { get; set; }

        /// <summary>
        /// Stone weight
        /// </summary>
        public float? StoneWeight { get; set; }

        /// <summary>
        /// Diamond height
        /// </summary>
        public float? DiamondHeight { get; set; }

        /// <summary>
        /// Box details
        /// </summary>
        public string? BoxDetails { get; set; }

        /// <summary>
        /// Product size
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Stone amount
        /// </summary>
        public decimal? StoneAmount { get; set; }

        /// <summary>
        /// Diamond amount
        /// </summary>
        public decimal? DiamondAmount { get; set; }

        /// <summary>
        /// Hallmark amount
        /// </summary>
        public decimal? HallmarkAmount { get; set; }

        /// <summary>
        /// Making charge per gram
        /// </summary>
        public decimal? MakingPerGram { get; set; }

        /// <summary>
        /// Making charge percentage
        /// </summary>
        public decimal? MakingPercentage { get; set; }

        /// <summary>
        /// Fixed making charge amount
        /// </summary>
        public decimal? MakingFixedAmount { get; set; }

        /// <summary>
        /// MRP (Maximum Retail Price)
        /// </summary>
        public decimal? Mrp { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
} 