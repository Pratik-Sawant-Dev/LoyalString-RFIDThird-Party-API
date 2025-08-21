using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for managing stock verification sessions and reports
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockVerificationController : ControllerBase
    {
        private readonly IStockVerificationService _stockVerificationService;

        public StockVerificationController(IStockVerificationService stockVerificationService)
        {
            _stockVerificationService = stockVerificationService;
        }

        /// <summary>
        /// Create a new stock verification session
        /// </summary>
        /// <param name="request">Stock verification session details</param>
        /// <returns>Created verification session</returns>
        /// <response code="200">Verification session created successfully</response>
        /// <response code="400">Invalid request or client code not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("create-session")]
        [ProducesResponseType(typeof(StockVerificationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationResponseDto>> CreateStockVerification([FromBody] CreateStockVerificationDto request)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.CreateStockVerificationAsync(request, clientCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Submit scanned items for verification (matched and unmatched)
        /// </summary>
        /// <param name="request">Scanned items with verification status</param>
        /// <returns>Updated verification session</returns>
        /// <response code="200">Items submitted successfully</response>
        /// <response code="400">Invalid request or verification session not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("submit-verification")]
        [ProducesResponseType(typeof(StockVerificationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationResponseDto>> SubmitStockVerification([FromBody] SubmitStockVerificationDto request)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.SubmitStockVerificationAsync(request, clientCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stock verification session by ID
        /// </summary>
        /// <param name="verificationId">Verification session ID</param>
        /// <returns>Verification session details</returns>
        /// <response code="200">Verification session retrieved successfully</response>
        /// <response code="400">Client code not found</response>
        /// <response code="404">Verification session not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{verificationId}")]
        [ProducesResponseType(typeof(StockVerificationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationResponseDto>> GetStockVerification(int verificationId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.GetStockVerificationByIdAsync(verificationId, clientCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all stock verifications with filters and pagination
        /// </summary>
        /// <param name="filter">Filter criteria and pagination</param>
        /// <returns>Paginated list of verification sessions</returns>
        /// <response code="200">Verifications retrieved successfully</response>
        /// <response code="400">Client code not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("list")]
        [ProducesResponseType(typeof(StockVerificationReportResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationReportResponseDto>> GetStockVerifications([FromQuery] StockVerificationReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.GetStockVerificationsAsync(filter, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get stock verification summary
        /// </summary>
        /// <returns>Verification summary statistics</returns>
        /// <response code="200">Summary retrieved successfully</response>
        /// <response code="400">Client code not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(StockVerificationSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationSummaryDto>> GetStockVerificationSummary()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.GetStockVerificationSummaryAsync(clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get date-wise stock verification report
        /// </summary>
        /// <param name="startDate">Start date for report</param>
        /// <param name="endDate">End date for report</param>
        /// <returns>Date-wise verification report</returns>
        /// <response code="200">Report generated successfully</response>
        /// <response code="400">Client code not found or invalid date range</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("date-wise-report")]
        [ProducesResponseType(typeof(List<DateWiseStockVerificationReportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<DateWiseStockVerificationReportDto>>> GetDateWiseStockVerificationReport(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                if (startDate > endDate)
                {
                    return BadRequest("Start date cannot be greater than end date");
                }

                var result = await _stockVerificationService.GetDateWiseStockVerificationReportAsync(startDate, endDate, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete a stock verification session
        /// </summary>
        /// <param name="verificationId">Verification session ID</param>
        /// <returns>Completed verification session</returns>
        /// <response code="200">Verification session completed successfully</response>
        /// <response code="400">Client code not found or invalid operation</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("{verificationId}/complete")]
        [ProducesResponseType(typeof(StockVerificationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationResponseDto>> CompleteStockVerification(int verificationId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.CompleteStockVerificationAsync(verificationId, clientCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a stock verification session
        /// </summary>
        /// <param name="verificationId">Verification session ID</param>
        /// <returns>Cancelled verification session</returns>
        /// <response code="200">Verification session cancelled successfully</response>
        /// <response code="400">Client code not found or invalid operation</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("{verificationId}/cancel")]
        [ProducesResponseType(typeof(StockVerificationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<StockVerificationResponseDto>> CancelStockVerification(int verificationId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                var result = await _stockVerificationService.CancelStockVerificationAsync(verificationId, clientCode);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get verification details by status (matched, unmatched, missing)
        /// </summary>
        /// <param name="verificationId">Verification session ID</param>
        /// <param name="status">Status filter (Matched, Unmatched, Missing)</param>
        /// <returns>List of verification details with specified status</returns>
        /// <response code="200">Details retrieved successfully</response>
        /// <response code="400">Client code not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{verificationId}/details/{status}")]
        [ProducesResponseType(typeof(List<StockVerificationDetailDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<StockVerificationDetailDto>>> GetVerificationDetailsByStatus(int verificationId, string status)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                {
                    return BadRequest("Client code not found in token");
                }

                if (!new[] { "Matched", "Unmatched", "Missing" }.Contains(status))
                {
                    return BadRequest("Invalid status. Must be one of: Matched, Unmatched, Missing");
                }

                var result = await _stockVerificationService.GetVerificationDetailsByStatusAsync(verificationId, status, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #region Private Methods

        private string? GetClientCodeFromToken()
        {
            return User.FindFirst("ClientCode")?.Value;
        }

        #endregion
    }
}
