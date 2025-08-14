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
} 