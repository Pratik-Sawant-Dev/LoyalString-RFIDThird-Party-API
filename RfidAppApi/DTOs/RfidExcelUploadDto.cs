using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    /// <summary>
    /// DTO for RFID Excel upload request
    /// </summary>
    public class RfidExcelUploadDto
    {
        /// <summary>
        /// Excel file containing RFID data
        /// </summary>
        [Required]
        public IFormFile ExcelFile { get; set; } = null!;
        
        /// <summary>
        /// Client code for the upload (optional, will be extracted from token if not provided)
        /// </summary>
        public string? ClientCode { get; set; }
        
        /// <summary>
        /// Whether to update existing RFID codes with new EPC values
        /// </summary>
        public bool UpdateExisting { get; set; } = true;
        
        /// <summary>
        /// Whether to create new RFID codes if they don't exist
        /// </summary>
        public bool CreateNew { get; set; } = true;
    }

    /// <summary>
    /// DTO for individual RFID data from Excel
    /// </summary>
    public class RfidExcelRowDto
    {
        /// <summary>
        /// RFID Code from Excel column 1
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RFIDCode { get; set; } = string.Empty;
        
        /// <summary>
        /// EPC Value from Excel column 2
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EPCValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for RFID Excel upload response
    /// </summary>
    public class RfidExcelUploadResponseDto
    {
        /// <summary>
        /// Total rows processed
        /// </summary>
        public int TotalRowsProcessed { get; set; }
        
        /// <summary>
        /// Number of new RFID codes created
        /// </summary>
        public int NewRfidsCreated { get; set; }
        
        /// <summary>
        /// Number of existing RFID codes updated
        /// </summary>
        public int ExistingRfidsUpdated { get; set; }
        
        /// <summary>
        /// Number of rows with errors
        /// </summary>
        public int ErrorRows { get; set; }
        
        /// <summary>
        /// List of errors encountered during processing
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// List of successfully processed rows
        /// </summary>
        public List<RfidExcelRowDto> ProcessedRows { get; set; } = new List<RfidExcelRowDto>();
        
        /// <summary>
        /// Summary message
        /// </summary>
        public string Summary { get; set; } = string.Empty;
    }
}
