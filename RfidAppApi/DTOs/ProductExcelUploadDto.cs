using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RfidAppApi.DTOs
{
    /// <summary>
    /// DTO for Product Excel upload request
    /// </summary>
    public class ProductExcelUploadDto
    {
        /// <summary>
        /// Excel file containing product data
        /// </summary>
        [Required]
        public IFormFile ExcelFile { get; set; } = null!;
        
        /// <summary>
        /// Whether to update existing products (by ItemCode)
        /// </summary>
        public bool UpdateExisting { get; set; } = false;
        
        /// <summary>
        /// Whether to skip rows with errors and continue processing
        /// </summary>
        public bool SkipErrors { get; set; } = true;
    }

    /// <summary>
    /// DTO for individual product data from Excel row
    /// </summary>
    public class ProductExcelRowDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string CounterName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public string PurityName { get; set; } = string.Empty;
        public string? RfidCode { get; set; }
        public string? BoxDetails { get; set; }
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
        public string? Status { get; set; }
        
        // Custom Fields - stored as JSON string in Excel, will be parsed
        public string? CustomFieldsJson { get; set; }
        
        // Excel row number for error reporting
        public int ExcelRowNumber { get; set; }
    }

    /// <summary>
    /// DTO for Product Excel upload response
    /// </summary>
    public class ProductExcelUploadResponseDto
    {
        public int TotalRowsProcessed { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int SuccessfullyUpdated { get; set; }
        public int ErrorRows { get; set; }
        public int SkippedRows { get; set; }
        public List<ExcelErrorDto> Errors { get; set; } = new List<ExcelErrorDto>();
        public string Summary { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
    }

    /// <summary>
    /// DTO for Excel error details
    /// </summary>
    public class ExcelErrorDto
    {
        public int RowNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

