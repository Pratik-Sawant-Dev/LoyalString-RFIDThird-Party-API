using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for managing stock transfers between branches, counters, and boxes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransferController : ControllerBase
    {
        private readonly IStockTransferService _stockTransferService;
        private readonly IClientService _clientService;

        public StockTransferController(IStockTransferService stockTransferService, IClientService clientService)
        {
            _stockTransferService = stockTransferService;
            _clientService = clientService;
        }

        /// <summary>
        /// Create a new stock transfer
        /// </summary>
        /// <param name="createDto">Transfer creation details</param>
        /// <returns>Created transfer details</returns>
        [HttpPost]
        public async Task<ActionResult<StockTransferResponseDto>> CreateTransfer([FromBody] CreateStockTransferDto createDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.CreateTransferAsync(createDto, clientCode);
                return CreatedAtAction(nameof(GetTransfer), new { transferId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple stock transfers in bulk
        /// </summary>
        /// <param name="bulkDto">Bulk transfer details</param>
        /// <returns>Bulk transfer results</returns>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkTransferResponseDto>> CreateBulkTransfers([FromBody] BulkStockTransferDto bulkDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.CreateBulkTransfersAsync(bulkDto, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating bulk transfers", details = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific transfer by ID
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <returns>Transfer details</returns>
        [HttpGet("{transferId}")]
        public async Task<ActionResult<StockTransferResponseDto>> GetTransfer(int transferId)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetTransferAsync(transferId, clientCode);

                if (result == null)
                    return NotFound(new { error = "Transfer not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all transfers with optional filtering
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>List of transfers</returns>
        [HttpGet]
        public async Task<ActionResult<List<StockTransferResponseDto>>> GetTransfers([FromQuery] TransferFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetTransfersAsync(filter, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving transfers", details = ex.Message });
            }
        }

        /// <summary>
        /// Update transfer status
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <param name="updateDto">Status update details</param>
        /// <returns>Updated transfer details</returns>
        [HttpPut("{transferId}/status")]
        public async Task<ActionResult<StockTransferResponseDto>> UpdateTransferStatus(int transferId, [FromBody] UpdateTransferStatusDto updateDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.UpdateTransferStatusAsync(transferId, updateDto, clientCode);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating transfer status", details = ex.Message });
            }
        }

        /// <summary>
        /// Approve a transfer
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <param name="approveDto">Approval details</param>
        /// <returns>Approved transfer details</returns>
        [HttpPut("{transferId}/approve")]
        public async Task<ActionResult<StockTransferResponseDto>> ApproveTransfer(int transferId, [FromBody] ApproveTransferDto approveDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.ApproveTransferAsync(transferId, approveDto, clientCode);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while approving the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Reject a transfer
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <param name="rejectDto">Rejection details</param>
        /// <returns>Rejected transfer details</returns>
        [HttpPut("{transferId}/reject")]
        public async Task<ActionResult<StockTransferResponseDto>> RejectTransfer(int transferId, [FromBody] RejectTransferDto rejectDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.RejectTransferAsync(transferId, rejectDto, clientCode);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while rejecting the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Complete a transfer (move product to destination)
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <param name="completedBy">User who completed the transfer</param>
        /// <returns>Completed transfer details</returns>
        [HttpPut("{transferId}/complete")]
        public async Task<ActionResult<StockTransferResponseDto>> CompleteTransfer(int transferId, [FromQuery] string completedBy)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.CompleteTransferAsync(transferId, completedBy, clientCode);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while completing the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a transfer
        /// </summary>
        /// <param name="transferId">Transfer ID</param>
        /// <param name="cancelledBy">User who cancelled the transfer</param>
        /// <returns>Cancelled transfer details</returns>
        [HttpPut("{transferId}/cancel")]
        public async Task<ActionResult<StockTransferResponseDto>> CancelTransfer(int transferId, [FromQuery] string cancelledBy)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.CancelTransferAsync(transferId, cancelledBy, clientCode);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while cancelling the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Get transfer summary and statistics
        /// </summary>
        /// <param name="fromDate">Start date for summary</param>
        /// <param name="toDate">End date for summary</param>
        /// <returns>Transfer summary</returns>
        [HttpGet("summary")]
        public async Task<ActionResult<TransferSummaryDto>> GetTransferSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetTransferSummaryAsync(clientCode, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving transfer summary", details = ex.Message });
            }
        }

        /// <summary>
        /// Get transfers by product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of transfers for the product</returns>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<StockTransferResponseDto>>> GetTransfersByProduct(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetTransfersByProductAsync(productId, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving product transfers", details = ex.Message });
            }
        }

        /// <summary>
        /// Get transfers by RFID
        /// </summary>
        /// <param name="rfidCode">RFID code</param>
        /// <returns>List of transfers for the RFID</returns>
        [HttpGet("rfid/{rfidCode}")]
        public async Task<ActionResult<List<StockTransferResponseDto>>> GetTransfersByRfid(string rfidCode)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetTransfersByRfidAsync(rfidCode, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving RFID transfers", details = ex.Message });
            }
        }

        /// <summary>
        /// Get pending transfers for a specific location
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <param name="counterId">Counter ID</param>
        /// <param name="boxId">Box ID (optional)</param>
        /// <returns>List of pending transfers</returns>
        [HttpGet("pending")]
        public async Task<ActionResult<List<StockTransferResponseDto>>> GetPendingTransfersByLocation(
            [FromQuery] int branchId, 
            [FromQuery] int counterId, 
            [FromQuery] int? boxId = null)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetPendingTransfersByLocationAsync(branchId, counterId, boxId, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving pending transfers", details = ex.Message });
            }
        }

        /// <summary>
        /// Validate if a transfer is possible
        /// </summary>
        /// <param name="createDto">Transfer creation details</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateTransfer([FromBody] CreateStockTransferDto createDto)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.ValidateTransferAsync(createDto, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while validating the transfer", details = ex.Message });
            }
        }

        /// <summary>
        /// Get transfer history for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Transfer history</returns>
        [HttpGet("history/{productId}")]
        public async Task<ActionResult<List<StockTransferResponseDto>>> GetProductTransferHistory(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromUser();
                var result = await _stockTransferService.GetProductTransferHistoryAsync(productId, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving transfer history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available transfer types
        /// </summary>
        /// <returns>List of transfer types</returns>
        [HttpGet("types")]
        public ActionResult<object> GetTransferTypes()
        {
            return Ok(new
            {
                types = new[]
                {
                    new { value = TransferTypes.Branch, label = "Branch Transfer" },
                    new { value = TransferTypes.Counter, label = "Counter Transfer" },
                    new { value = TransferTypes.Box, label = "Box Transfer" },
                    new { value = TransferTypes.Mixed, label = "Mixed Transfer" }
                }
            });
        }

        /// <summary>
        /// Get available transfer statuses
        /// </summary>
        /// <returns>List of transfer statuses</returns>
        [HttpGet("statuses")]
        public ActionResult<object> GetTransferStatuses()
        {
            return Ok(new
            {
                statuses = new[]
                {
                    new { value = TransferStatuses.Pending, label = "Pending" },
                    new { value = TransferStatuses.InTransit, label = "In Transit" },
                    new { value = TransferStatuses.Completed, label = "Completed" },
                    new { value = TransferStatuses.Cancelled, label = "Cancelled" },
                    new { value = TransferStatuses.Rejected, label = "Rejected" }
                }
            });
        }

        #region Private Helper Methods

        /// <summary>
        /// Extract client code from user claims
        /// </summary>
        /// <returns>Client code</returns>
        private string GetClientCodeFromUser()
        {
            // This should be implemented based on your authentication mechanism
            // For now, returning a default value - you should implement this properly
            var clientCodeClaim = User.FindFirst("ClientCode");
            if (clientCodeClaim != null)
                return clientCodeClaim.Value;

            // Fallback to query string or header if available
            var clientCodeFromQuery = Request.Query["clientCode"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientCodeFromQuery))
                return clientCodeFromQuery;

            var clientCodeFromHeader = Request.Headers["X-Client-Code"].FirstOrDefault();
            if (!string.IsNullOrEmpty(clientCodeFromHeader))
                return clientCodeFromHeader;

            throw new InvalidOperationException("Client code not found in user claims, query string, or headers");
        }

        #endregion
    }
}
