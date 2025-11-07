using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for Product Excel operations (Bulk Upload and Template Download)
    /// Optimized for processing 100,000+ products efficiently
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductExcelController : ControllerBase
    {
        private readonly IProductExcelService _productExcelService;
        private readonly ILogger<ProductExcelController> _logger;

        public ProductExcelController(
            IProductExcelService productExcelService,
            ILogger<ProductExcelController> logger)
        {
            _productExcelService = productExcelService;
            _logger = logger;
        }

        /// <summary>
        /// Download Excel template for bulk product upload
        /// </summary>
        /// <returns>Excel template file</returns>
        /// <response code="200">Excel template downloaded successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("download-template")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var templateBytes = await _productExcelService.GenerateExcelTemplateAsync();
                
                return File(
                    templateBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Product_Upload_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel template");
                return StatusCode(500, new { error = "Failed to generate Excel template", message = ex.Message });
            }
        }

        /// <summary>
        /// Upload products from Excel file
        /// Supports bulk upload of up to 100,000 products efficiently
        /// </summary>
        /// <param name="uploadDto">Excel upload request with file and options</param>
        /// <returns>Upload processing results</returns>
        /// <response code="200">Excel upload processed successfully</response>
        /// <response code="400">Invalid request or validation errors</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ProductExcelUploadResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductExcelUploadResponseDto>> UploadProductsFromExcel([FromForm] ProductExcelUploadDto uploadDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                // Validate file
                if (uploadDto.ExcelFile == null || uploadDto.ExcelFile.Length == 0)
                {
                    return BadRequest(new { message = "Excel file is required." });
                }

                // Check file extension
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(uploadDto.ExcelFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Only Excel files (.xlsx, .xls) are allowed." });
                }

                // Check file size (max 50MB for large files)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (uploadDto.ExcelFile.Length > maxFileSize)
                {
                    return BadRequest(new { message = $"File size cannot exceed {maxFileSize / (1024 * 1024)}MB." });
                }

                _logger.LogInformation("Starting product Excel upload for client: {ClientCode}, File: {FileName}, Size: {Size} bytes", 
                    clientCode, uploadDto.ExcelFile.FileName, uploadDto.ExcelFile.Length);

                var result = await _productExcelService.UploadProductsFromExcelAsync(uploadDto, clientCode);

                _logger.LogInformation("Product Excel upload completed. {Summary}", result.Summary);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during product Excel upload");
                return StatusCode(500, new { error = "Failed to process Excel file", message = ex.Message });
            }
        }

        /// <summary>
        /// Export all products to Excel file with all fields
        /// </summary>
        /// <returns>Excel file containing all products</returns>
        /// <response code="200">Excel file downloaded successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("export-all")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportAllProducts()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                _logger.LogInformation("Starting product export to Excel for client: {ClientCode}", clientCode);

                var excelBytes = await _productExcelService.ExportAllProductsToExcelAsync(clientCode);
                
                var fileName = $"Products_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                
                return File(
                    excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting products to Excel");
                return StatusCode(500, new { error = "Failed to export products to Excel", message = ex.Message });
            }
        }

        /// <summary>
        /// Get client code from JWT token
        /// </summary>
        private string? GetClientCodeFromToken()
        {
            var clientCodeClaim = User.FindFirst("ClientCode");
            return clientCodeClaim?.Value;
        }
    }
}

