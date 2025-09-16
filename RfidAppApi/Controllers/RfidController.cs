using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using RfidAppApi.Data;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// RFID management controller for handling RFID tag operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RfidController : ControllerBase
    {
        private readonly IRfidService _rfidService;
        private readonly IRfidExcelService _rfidExcelService;
        private readonly IClientService _clientService;

        public RfidController(IRfidService rfidService, IRfidExcelService rfidExcelService, IClientService clientService)
        {
            _rfidService = rfidService;
            _rfidExcelService = rfidExcelService;
            _clientService = clientService;
        }

        /// <summary>
        /// Get all RFID tags for the authenticated client
        /// </summary>
        /// <returns>List of all RFID tags for the client</returns>
        /// <response code="200">RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetAllRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetRfidsByClientAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get RFID tag by code for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <returns>RFID tag information</returns>
        /// <response code="200">RFID tag found</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{rfidCode}")]
        [ProducesResponseType(typeof(RfidDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> GetRfidByCode(string rfidCode)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfid = await _rfidService.GetRfidByCodeAsync(rfidCode, clientCode);
                if (rfid == null)
                {
                    return NotFound(new { message = $"RFID tag with code {rfidCode} not found." });
                }

                return Ok(rfid);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new RFID tag for the authenticated client
        /// </summary>
        /// <param name="createRfidDto">RFID tag creation details</param>
        /// <returns>Created RFID tag information</returns>
        /// <response code="201">RFID tag created successfully</response>
        /// <response code="400">Invalid input or client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(RfidDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> CreateRfid([FromBody] CreateRfidDto createRfidDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                // Set the client code from the token
                createRfidDto.ClientCode = clientCode;

                var createdRfid = await _rfidService.CreateRfidAsync(createRfidDto);
                return CreatedAtAction(nameof(GetRfidByCode), new { rfidCode = createdRfid.RFIDCode }, createdRfid);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing RFID tag for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <param name="updateRfidDto">Updated RFID tag information</param>
        /// <returns>Updated RFID tag information</returns>
        /// <response code="200">RFID tag updated successfully</response>
        /// <response code="400">Invalid input or client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{rfidCode}")]
        [ProducesResponseType(typeof(RfidDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidDto>> UpdateRfid(string rfidCode, [FromBody] UpdateRfidDto updateRfidDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var updatedRfid = await _rfidService.UpdateRfidAsync(rfidCode, clientCode, updateRfidDto);
                return Ok(updatedRfid);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an RFID tag for the authenticated client
        /// </summary>
        /// <param name="rfidCode">RFID tag code</param>
        /// <returns>No content</returns>
        /// <response code="204">RFID tag deleted successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="404">RFID tag not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{rfidCode}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteRfid(string rfidCode)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                await _rfidService.DeleteRfidAsync(rfidCode, clientCode);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the RFID tag.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available RFID tags (not assigned to any product) for the authenticated client
        /// </summary>
        /// <returns>List of available RFID tags</returns>
        /// <response code="200">Available RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetAvailableRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetAvailableRfidsAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving available RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active RFID tags for the authenticated client
        /// </summary>
        /// <returns>List of active RFID tags</returns>
        /// <response code="200">Active RFID tags retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<RfidDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<RfidDto>>> GetActiveRfids()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var rfids = await _rfidService.GetActiveRfidsAsync(clientCode);
                return Ok(rfids);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active RFID tags.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get RFID count for the authenticated client
        /// </summary>
        /// <returns>Total count of RFID tags</returns>
        /// <response code="200">RFID count retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("count")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<int>> GetRfidCount()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var count = await _rfidService.GetRfidCountByClientAsync(clientCode);
                return Ok(new { clientCode, count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving RFID count.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed analysis of used RFID tags (assigned to products) for the authenticated client
        /// </summary>
        /// <returns>Used RFID analysis with count and detailed information</returns>
        /// <response code="200">Used RFID analysis retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("used-analysis")]
        [ProducesResponseType(typeof(UsedRfidAnalysisDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UsedRfidAnalysisDto>> GetUsedRfidAnalysis()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var usedAnalysis = await _rfidService.GetUsedRfidAnalysisAsync(clientCode);
                return Ok(usedAnalysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving used RFID analysis.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed analysis of unused RFID tags (not assigned to products) for the authenticated client
        /// </summary>
        /// <returns>Unused RFID analysis with count and detailed information</returns>
        /// <response code="200">Unused RFID analysis retrieved successfully</response>
        /// <response code="400">Client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("unused-analysis")]
        [ProducesResponseType(typeof(UnusedRfidAnalysisDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UnusedRfidAnalysisDto>> GetUnusedRfidAnalysis()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest(new { message = "Client code not found in token." });
                }

                var unusedAnalysis = await _rfidService.GetUnusedRfidAnalysisAsync(clientCode);
                return Ok(unusedAnalysis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving unused RFID analysis.", error = ex.Message });
            }
        }

        /// <summary>
        /// Scan for products by EPC value - returns all products associated with the scanned EPC value(s)
        /// </summary>
        /// <param name="request">Scan request containing single EPC value or multiple EPC values</param>
        /// <returns>Scan response with all associated products grouped by EPC value</returns>
        /// <response code="200">Scan completed successfully</response>
        /// <response code="400">Invalid request or client code not found in token</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("scan")]
        [ProducesResponseType(typeof(RfidScanResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidScanResponseDto>> ScanProductsByEpcValue([FromBody] RfidScanRequestDto request)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _rfidService.ScanProductsByEpcValueAsync(request, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Upload RFID data from Excel file
        /// </summary>
        /// <param name="uploadDto">Excel upload request with file and options</param>
        /// <returns>Upload processing results</returns>
        /// <response code="200">Excel upload processed successfully</response>
        /// <response code="400">Invalid request or validation errors</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("upload-excel")]
        [ProducesResponseType(typeof(RfidExcelUploadResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<RfidExcelUploadResponseDto>> UploadRfidFromExcel([FromForm] RfidExcelUploadDto uploadDto)
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

                // Check file size (max 10MB)
                if (uploadDto.ExcelFile.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size cannot exceed 10MB." });
                }

                var result = await _rfidExcelService.UploadRfidFromExcelAsync(uploadDto, clientCode);
                
                return Ok(new
                {
                    success = true,
                    message = "Excel upload processed successfully",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the Excel file",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Download Excel template for RFID upload
        /// </summary>
        /// <returns>Excel template file</returns>
        /// <response code="200">Template downloaded successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("download-template")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DownloadExcelTemplate()
        {
            try
            {
                var templateBytes = await _rfidExcelService.GenerateExcelTemplateAsync();
                
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RFID_Upload_Template.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while generating the template",
                    error = ex.Message
                });
            }
        }

        private string GetClientCodeFromToken()
        {
            var clientCodeClaim = User.Claims.FirstOrDefault(c => c.Type == "ClientCode");
            return clientCodeClaim?.Value ?? string.Empty;
        }
    }
} 