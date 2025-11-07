using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for comprehensive reporting functionality
    /// Provides APIs for stock movements, sales reports, stock summaries, daily balances, and RFID usage
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(
            IReportingService reportingService,
            ILogger<ReportingController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        #region Helper Methods

        /// <summary>
        /// Get client code from JWT token
        /// </summary>
        private string? GetClientCodeFromToken()
        {
            return User.FindFirst("ClientCode")?.Value;
        }

        /// <summary>
        /// Standard error response
        /// </summary>
        private ObjectResult ErrorResponse(int statusCode, string message, Exception? ex = null)
        {
            _logger.LogError(ex, "Error in ReportingController: {Message}", message);
            return StatusCode(statusCode, new
            {
                success = false,
                message = message,
                error = ex?.Message
            });
        }

        /// <summary>
        /// Standard success response
        /// </summary>
        private OkObjectResult SuccessResponse(object data, string? message = null)
        {
            return Ok(new
            {
                success = true,
                message = message,
                data = data
            });
        }

        #endregion

        #region Stock Movement APIs

        /// <summary>
        /// Create a new stock movement
        /// </summary>
        /// <param name="movementDto">Stock movement data</param>
        /// <returns>Created stock movement</returns>
        [HttpPost("stock-movements")]
        [ProducesResponseType(typeof(StockMovementDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateStockMovement([FromBody] CreateStockMovementDto movementDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var result = await _reportingService.CreateStockMovementAsync(movementDto, clientCode);
                return CreatedAtAction(
                    nameof(GetStockMovementById),
                    new { movementId = result.Id },
                    new { success = true, message = "Stock movement created successfully", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to create stock movement", ex);
            }
        }

        /// <summary>
        /// Create multiple stock movements in bulk
        /// </summary>
        /// <param name="bulkDto">Bulk stock movement data</param>
        /// <returns>List of created stock movements</returns>
        [HttpPost("stock-movements/bulk")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateBulkStockMovements([FromBody] BulkStockMovementDto bulkDto)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var results = await _reportingService.CreateBulkStockMovementsAsync(bulkDto, clientCode);
                return SuccessResponse(results, $"Successfully created {results.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to create bulk stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movements with filters
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovements([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return SuccessResponse(movements, $"Found {movements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movement by ID
        /// </summary>
        /// <param name="movementId">Stock movement ID</param>
        /// <returns>Stock movement details</returns>
        [HttpGet("stock-movements/{movementId}")]
        [ProducesResponseType(typeof(StockMovementDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementById(int movementId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var movement = await _reportingService.GetStockMovementByIdAsync(movementId, clientCode);
                if (movement == null)
                    return NotFound(new { success = false, message = $"Stock movement with ID {movementId} not found" });

                return SuccessResponse(movement);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movement", ex);
            }
        }

        /// <summary>
        /// Get stock movements by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements/range")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return SuccessResponse(movements, $"Found {movements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movements by product ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements/product/{productId}")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementsByProduct(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto { };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                var productMovements = movements.Where(m => m.ProductId == productId).ToList();
                return SuccessResponse(productMovements, $"Found {productMovements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movements by branch ID
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements/branch/{branchId}")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementsByBranch(int branchId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto { BranchId = branchId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return SuccessResponse(movements, $"Found {movements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movements by counter ID
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements/counter/{counterId}")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementsByCounter(int counterId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto { CounterId = counterId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return SuccessResponse(movements, $"Found {movements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        /// <summary>
        /// Get stock movements by category ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of stock movements</returns>
        [HttpGet("stock-movements/category/{categoryId}")]
        [ProducesResponseType(typeof(List<StockMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockMovementsByCategory(int categoryId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto { CategoryId = categoryId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return SuccessResponse(movements, $"Found {movements.Count} stock movement(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock movements", ex);
            }
        }

        #endregion

        #region Daily Stock Balance APIs

        /// <summary>
        /// Get daily stock balance for a specific product and date
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="date">Balance date</param>
        /// <returns>Daily stock balance</returns>
        [HttpGet("daily-balances/{productId}/{date:datetime}")]
        [ProducesResponseType(typeof(DailyStockBalanceDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyStockBalance(int productId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var balance = await _reportingService.GetDailyStockBalanceAsync(productId, date, clientCode);
                return SuccessResponse(balance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily stock balance", ex);
            }
        }

        /// <summary>
        /// Get daily stock balances with filters
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>List of daily stock balances</returns>
        [HttpGet("daily-balances")]
        [ProducesResponseType(typeof(List<DailyStockBalanceDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyStockBalances([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var balances = await _reportingService.GetDailyStockBalancesAsync(filter, clientCode);
                return SuccessResponse(balances, $"Found {balances.Count} daily balance(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily stock balances", ex);
            }
        }

        /// <summary>
        /// Calculate daily stock balance for a specific product and date
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="date">Balance date</param>
        /// <returns>Calculated daily stock balance</returns>
        [HttpPost("daily-balances/calculate/{productId}/{date:datetime}")]
        [ProducesResponseType(typeof(DailyStockBalanceDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CalculateDailyStockBalance(int productId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var balance = await _reportingService.CalculateDailyStockBalanceAsync(productId, date, clientCode);
                return SuccessResponse(balance, "Daily stock balance calculated successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to calculate daily stock balance", ex);
            }
        }

        /// <summary>
        /// Calculate daily stock balances for all products on a specific date
        /// </summary>
        /// <param name="date">Balance date</param>
        /// <returns>List of calculated daily stock balances</returns>
        [HttpPost("daily-balances/calculate/{date:datetime}")]
        [ProducesResponseType(typeof(List<DailyStockBalanceDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CalculateDailyStockBalances(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var balances = await _reportingService.CalculateDailyStockBalancesAsync(date, clientCode);
                return SuccessResponse(balances, $"Calculated {balances.Count} daily balance(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to calculate daily stock balances", ex);
            }
        }

        /// <summary>
        /// Get daily stock balances by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of daily stock balances</returns>
        [HttpGet("daily-balances/range")]
        [ProducesResponseType(typeof(List<DailyStockBalanceDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyStockBalancesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var filter = new ReportFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var balances = await _reportingService.GetDailyStockBalancesAsync(filter, clientCode);
                return SuccessResponse(balances, $"Found {balances.Count} daily balance(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily stock balances", ex);
            }
        }

        #endregion

        #region Sales Report APIs

        /// <summary>
        /// Get sales report with filters
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>List of sales reports</returns>
        [HttpGet("sales")]
        [ProducesResponseType(typeof(List<SalesReportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var reports = await _reportingService.GetSalesReportAsync(filter, clientCode);
                return SuccessResponse(reports, $"Found {reports.Count} sales report(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        /// <summary>
        /// Get sales report for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>Sales report</returns>
        [HttpGet("sales/date/{date:datetime}")]
        [ProducesResponseType(typeof(SalesReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReportByDate(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetSalesReportByDateAsync(date, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        /// <summary>
        /// Get sales report for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of sales reports</returns>
        [HttpGet("sales/range")]
        [ProducesResponseType(typeof(List<SalesReportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReportByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var reports = await _reportingService.GetSalesReportByDateRangeAsync(startDate, endDate, clientCode);
                return SuccessResponse(reports, $"Found {reports.Count} sales report(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        /// <summary>
        /// Get sales report for a specific branch on a date
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Sales report</returns>
        [HttpGet("sales/branch/{branchId}/{date:datetime}")]
        [ProducesResponseType(typeof(SalesReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReportByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetSalesReportByBranchAsync(date, branchId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        /// <summary>
        /// Get sales report for a specific counter on a date
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Sales report</returns>
        [HttpGet("sales/counter/{counterId}/{date:datetime}")]
        [ProducesResponseType(typeof(SalesReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReportByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetSalesReportByCounterAsync(date, counterId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        /// <summary>
        /// Get sales report for a specific category on a date
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Sales report</returns>
        [HttpGet("sales/category/{categoryId}/{date:datetime}")]
        [ProducesResponseType(typeof(SalesReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesReportByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetSalesReportByCategoryAsync(date, categoryId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve sales report", ex);
            }
        }

        #endregion

        #region Stock Summary Report APIs

        /// <summary>
        /// Get stock summary report with filters
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>List of stock summary reports</returns>
        [HttpGet("stock-summary")]
        [ProducesResponseType(typeof(List<StockSummaryReportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockSummaryReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var reports = await _reportingService.GetStockSummaryReportAsync(filter, clientCode);
                return SuccessResponse(reports, $"Found {reports.Count} stock summary report(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock summary report", ex);
            }
        }

        /// <summary>
        /// Get stock summary for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>Stock summary report</returns>
        [HttpGet("stock-summary/date/{date:datetime}")]
        [ProducesResponseType(typeof(StockSummaryReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockSummaryByDate(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetStockSummaryByDateAsync(date, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock summary report", ex);
            }
        }

        /// <summary>
        /// Get stock summary for a specific branch on a date
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Stock summary report</returns>
        [HttpGet("stock-summary/branch/{branchId}/{date:datetime}")]
        [ProducesResponseType(typeof(StockSummaryReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockSummaryByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetStockSummaryByBranchAsync(date, branchId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock summary report", ex);
            }
        }

        /// <summary>
        /// Get stock summary for a specific counter on a date
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Stock summary report</returns>
        [HttpGet("stock-summary/counter/{counterId}/{date:datetime}")]
        [ProducesResponseType(typeof(StockSummaryReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockSummaryByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetStockSummaryByCounterAsync(date, counterId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock summary report", ex);
            }
        }

        /// <summary>
        /// Get stock summary for a specific category on a date
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Stock summary report</returns>
        [HttpGet("stock-summary/category/{categoryId}/{date:datetime}")]
        [ProducesResponseType(typeof(StockSummaryReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockSummaryByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetStockSummaryByCategoryAsync(date, categoryId, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve stock summary report", ex);
            }
        }

        #endregion

        #region Daily Activity Report APIs

        /// <summary>
        /// Get daily activity report with filters
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>List of daily activity reports</returns>
        [HttpGet("daily-activity")]
        [ProducesResponseType(typeof(List<DailyActivityReportDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyActivityReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var activities = await _reportingService.GetDailyActivityReportAsync(filter, clientCode);
                return SuccessResponse(activities, $"Found {activities.Count} daily activity report(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily activity report", ex);
            }
        }

        /// <summary>
        /// Get daily activity for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>Daily activity report</returns>
        [HttpGet("daily-activity/date/{date:datetime}")]
        [ProducesResponseType(typeof(DailyActivityReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyActivityByDate(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var activity = await _reportingService.GetDailyActivityByDateAsync(date, clientCode);
                return SuccessResponse(activity);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily activity report", ex);
            }
        }

        /// <summary>
        /// Get daily activity for a specific branch on a date
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Daily activity report</returns>
        [HttpGet("daily-activity/branch/{branchId}/{date:datetime}")]
        [ProducesResponseType(typeof(DailyActivityReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyActivityByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var activity = await _reportingService.GetDailyActivityByBranchAsync(date, branchId, clientCode);
                return SuccessResponse(activity);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily activity report", ex);
            }
        }

        /// <summary>
        /// Get daily activity for a specific counter on a date
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Daily activity report</returns>
        [HttpGet("daily-activity/counter/{counterId}/{date:datetime}")]
        [ProducesResponseType(typeof(DailyActivityReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyActivityByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var activity = await _reportingService.GetDailyActivityByCounterAsync(date, counterId, clientCode);
                return SuccessResponse(activity);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily activity report", ex);
            }
        }

        /// <summary>
        /// Get daily activity for a specific category on a date
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Daily activity report</returns>
        [HttpGet("daily-activity/category/{categoryId}/{date:datetime}")]
        [ProducesResponseType(typeof(DailyActivityReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDailyActivityByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var activity = await _reportingService.GetDailyActivityByCategoryAsync(date, categoryId, clientCode);
                return SuccessResponse(activity);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve daily activity report", ex);
            }
        }

        #endregion

        #region Report Summary APIs

        /// <summary>
        /// Get report summary for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>Report summary</returns>
        [HttpGet("summary/{date:datetime}")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportSummary(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var summary = await _reportingService.GetReportSummaryAsync(date, clientCode);
                return SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve report summary", ex);
            }
        }

        /// <summary>
        /// Get report summary for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Report summary</returns>
        [HttpGet("summary/range")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportSummaryByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var summary = await _reportingService.GetReportSummaryByDateRangeAsync(startDate, endDate, clientCode);
                return SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve report summary", ex);
            }
        }

        /// <summary>
        /// Get report summary for a specific branch on a date
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Report summary</returns>
        [HttpGet("summary/branch/{branchId}/{date:datetime}")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportSummaryByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var summary = await _reportingService.GetReportSummaryByBranchAsync(date, branchId, clientCode);
                return SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve report summary", ex);
            }
        }

        /// <summary>
        /// Get report summary for a specific counter on a date
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <param name="date">Report date</param>
        /// <returns>Report summary</returns>
        [HttpGet("summary/counter/{counterId}/{date:datetime}")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportSummaryByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var summary = await _reportingService.GetReportSummaryByCounterAsync(date, counterId, clientCode);
                return SuccessResponse(summary);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve report summary", ex);
            }
        }

        #endregion

        #region Stock Tracking APIs

        /// <summary>
        /// Get current stock for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Current stock quantity</returns>
        [HttpGet("stock/current/{productId}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStock(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var stock = await _reportingService.GetCurrentStockAsync(productId, clientCode);
                return SuccessResponse(stock, $"Current stock: {stock}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock", ex);
            }
        }

        /// <summary>
        /// Get current stock for a specific product by branch
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>Current stock quantity</returns>
        [HttpGet("stock/current/{productId}/branch/{branchId}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockByBranch(int productId, int branchId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var stock = await _reportingService.GetCurrentStockByBranchAsync(productId, branchId, clientCode);
                return SuccessResponse(stock, $"Current stock: {stock}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock", ex);
            }
        }

        /// <summary>
        /// Get current stock for a specific product by counter
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="counterId">Counter ID</param>
        /// <returns>Current stock quantity</returns>
        [HttpGet("stock/current/{productId}/counter/{counterId}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockByCounter(int productId, int counterId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var stock = await _reportingService.GetCurrentStockByCounterAsync(productId, counterId, clientCode);
                return SuccessResponse(stock, $"Current stock: {stock}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock", ex);
            }
        }

        /// <summary>
        /// Get current stock for a specific category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Current stock quantity</returns>
        [HttpGet("stock/current/category/{categoryId}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockByCategory(int categoryId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var stock = await _reportingService.GetCurrentStockByCategoryAsync(categoryId, clientCode);
                return SuccessResponse(stock, $"Current stock: {stock}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock", ex);
            }
        }

        /// <summary>
        /// Get current stock value for a specific product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Current stock value</returns>
        [HttpGet("stock/value/{productId}")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockValue(int productId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var value = await _reportingService.GetCurrentStockValueAsync(productId, clientCode);
                return SuccessResponse(value, $"Current stock value: {value:C}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock value", ex);
            }
        }

        /// <summary>
        /// Get current stock value for a specific product by branch
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="branchId">Branch ID</param>
        /// <returns>Current stock value</returns>
        [HttpGet("stock/value/{productId}/branch/{branchId}")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockValueByBranch(int productId, int branchId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var value = await _reportingService.GetCurrentStockValueByBranchAsync(productId, branchId, clientCode);
                return SuccessResponse(value, $"Current stock value: {value:C}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock value", ex);
            }
        }

        /// <summary>
        /// Get current stock value for a specific product by counter
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="counterId">Counter ID</param>
        /// <returns>Current stock value</returns>
        [HttpGet("stock/value/{productId}/counter/{counterId}")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockValueByCounter(int productId, int counterId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var value = await _reportingService.GetCurrentStockValueByCounterAsync(productId, counterId, clientCode);
                return SuccessResponse(value, $"Current stock value: {value:C}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock value", ex);
            }
        }

        /// <summary>
        /// Get current stock value for a specific category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Current stock value</returns>
        [HttpGet("stock/value/category/{categoryId}")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCurrentStockValueByCategory(int categoryId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var value = await _reportingService.GetCurrentStockValueByCategoryAsync(categoryId, clientCode);
                return SuccessResponse(value, $"Current stock value: {value:C}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve current stock value", ex);
            }
        }

        #endregion

        #region Utility APIs

        /// <summary>
        /// Process daily stock balances for a specific date
        /// </summary>
        /// <param name="date">Date to process</param>
        /// <returns>Processing result</returns>
        [HttpPost("process-balances/{date:datetime}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ProcessDailyStockBalances(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var result = await _reportingService.ProcessDailyStockBalancesAsync(date, clientCode);
                return SuccessResponse(result, result ? "Daily stock balances processed successfully" : "Failed to process daily stock balances");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to process daily stock balances", ex);
            }
        }

        /// <summary>
        /// Process daily stock balances for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Processing result</returns>
        [HttpPost("process-balances/range")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ProcessAllDailyStockBalances(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var result = await _reportingService.ProcessAllDailyStockBalancesAsync(startDate, endDate, clientCode);
                return SuccessResponse(result, result ? "Daily stock balances processed successfully" : "Failed to process daily stock balances");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to process daily stock balances", ex);
            }
        }

        /// <summary>
        /// Recalculate stock balances for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Recalculation result</returns>
        [HttpPost("recalculate-balances")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RecalculateStockBalances(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var result = await _reportingService.RecalculateStockBalancesAsync(startDate, endDate, clientCode);
                return SuccessResponse(result, result ? "Stock balances recalculated successfully" : "Failed to recalculate stock balances");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to recalculate stock balances", ex);
            }
        }

        #endregion

        #region RFID Usage Report APIs

        /// <summary>
        /// Get comprehensive RFID usage report
        /// </summary>
        /// <returns>RFID usage report</returns>
        [HttpGet("rfid-usage")]
        [ProducesResponseType(typeof(RfidUsageReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageReport()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetRfidUsageReportAsync(clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage report", ex);
            }
        }

        /// <summary>
        /// Get RFID usage report for a specific date
        /// </summary>
        /// <param name="date">Report date</param>
        /// <returns>RFID usage report</returns>
        [HttpGet("rfid-usage/date/{date:datetime}")]
        [ProducesResponseType(typeof(RfidUsageReportDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageReportByDate(DateTime date)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var report = await _reportingService.GetRfidUsageReportByDateAsync(date, clientCode);
                return SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage report", ex);
            }
        }

        /// <summary>
        /// Get all used RFID tags
        /// </summary>
        /// <returns>List of used RFID tags</returns>
        [HttpGet("rfid-usage/used")]
        [ProducesResponseType(typeof(List<RfidUsageDetailDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUsedRfidTags()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var usedTags = await _reportingService.GetUsedRfidTagsAsync(clientCode);
                return SuccessResponse(usedTags, $"Found {usedTags.Count} used RFID tag(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve used RFID tags", ex);
            }
        }

        /// <summary>
        /// Get all unused RFID tags
        /// </summary>
        /// <returns>List of unused RFID tags</returns>
        [HttpGet("rfid-usage/unused")]
        [ProducesResponseType(typeof(List<RfidUsageDetailDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUnusedRfidTags()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var unusedTags = await _reportingService.GetUnusedRfidTagsAsync(clientCode);
                return SuccessResponse(unusedTags, $"Found {unusedTags.Count} unused RFID tag(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve unused RFID tags", ex);
            }
        }

        /// <summary>
        /// Get RFID tags by usage status
        /// </summary>
        /// <param name="isUsed">Usage status (true for used, false for unused)</param>
        /// <returns>List of RFID tags</returns>
        [HttpGet("rfid-usage/status/{isUsed:bool}")]
        [ProducesResponseType(typeof(List<RfidUsageDetailDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidTagsByStatus(bool isUsed)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var tags = await _reportingService.GetRfidTagsByStatusAsync(isUsed, clientCode);
                return SuccessResponse(tags, $"Found {tags.Count} RFID tag(s)");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID tags", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary by category
        /// </summary>
        /// <returns>List of RFID usage by category</returns>
        [HttpGet("rfid-usage/by-category")]
        [ProducesResponseType(typeof(List<RfidUsageByCategoryDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByCategory()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var categoryUsage = await _reportingService.GetRfidUsageByCategoryAsync(clientCode);
                return SuccessResponse(categoryUsage, $"Found {categoryUsage.Count} category/categories");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by category", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary by branch
        /// </summary>
        /// <returns>List of RFID usage by branch</returns>
        [HttpGet("rfid-usage/by-branch")]
        [ProducesResponseType(typeof(List<RfidUsageByBranchDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByBranch()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var branchUsage = await _reportingService.GetRfidUsageByBranchAsync(clientCode);
                return SuccessResponse(branchUsage, $"Found {branchUsage.Count} branch/branches");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by branch", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary by counter
        /// </summary>
        /// <returns>List of RFID usage by counter</returns>
        [HttpGet("rfid-usage/by-counter")]
        [ProducesResponseType(typeof(List<RfidUsageByCounterDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByCounter()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var counterUsage = await _reportingService.GetRfidUsageByCounterAsync(clientCode);
                return SuccessResponse(counterUsage, $"Found {counterUsage.Count} counter/counters");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by counter", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>RFID usage by category</returns>
        [HttpGet("rfid-usage/category/{categoryId}")]
        [ProducesResponseType(typeof(RfidUsageByCategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByCategoryId(int categoryId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var categoryUsage = await _reportingService.GetRfidUsageByCategoryIdAsync(categoryId, clientCode);
                return SuccessResponse(categoryUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by category", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific branch
        /// </summary>
        /// <param name="branchId">Branch ID</param>
        /// <returns>RFID usage by branch</returns>
        [HttpGet("rfid-usage/branch/{branchId}")]
        [ProducesResponseType(typeof(RfidUsageByBranchDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByBranchId(int branchId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var branchUsage = await _reportingService.GetRfidUsageByBranchIdAsync(branchId, clientCode);
                return SuccessResponse(branchUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by branch", ex);
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific counter
        /// </summary>
        /// <param name="counterId">Counter ID</param>
        /// <returns>RFID usage by counter</returns>
        [HttpGet("rfid-usage/counter/{counterId}")]
        [ProducesResponseType(typeof(RfidUsageByCounterDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsageByCounterId(int counterId)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var counterUsage = await _reportingService.GetRfidUsageByCounterIdAsync(counterId, clientCode);
                return SuccessResponse(counterUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage by counter", ex);
            }
        }

        /// <summary>
        /// Get total count of RFID tags
        /// </summary>
        /// <returns>Total RFID tags count</returns>
        [HttpGet("rfid-usage/count/total")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTotalRfidTagsCount()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var count = await _reportingService.GetTotalRfidTagsCountAsync(clientCode);
                return SuccessResponse(count, $"Total RFID tags: {count}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve total RFID tags count", ex);
            }
        }

        /// <summary>
        /// Get count of used RFID tags
        /// </summary>
        /// <returns>Used RFID tags count</returns>
        [HttpGet("rfid-usage/count/used")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUsedRfidTagsCount()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var count = await _reportingService.GetUsedRfidTagsCountAsync(clientCode);
                return SuccessResponse(count, $"Used RFID tags: {count}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve used RFID tags count", ex);
            }
        }

        /// <summary>
        /// Get count of unused RFID tags
        /// </summary>
        /// <returns>Unused RFID tags count</returns>
        [HttpGet("rfid-usage/count/unused")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUnusedRfidTagsCount()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var count = await _reportingService.GetUnusedRfidTagsCountAsync(clientCode);
                return SuccessResponse(count, $"Unused RFID tags: {count}");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve unused RFID tags count", ex);
            }
        }

        /// <summary>
        /// Get RFID usage percentage
        /// </summary>
        /// <returns>RFID usage percentage</returns>
        [HttpGet("rfid-usage/percentage")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRfidUsagePercentage()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var percentage = await _reportingService.GetRfidUsagePercentageAsync(clientCode);
                return SuccessResponse(percentage, $"RFID usage: {percentage:F2}%");
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to retrieve RFID usage percentage", ex);
            }
        }

        #endregion

        #region Export APIs

        /// <summary>
        /// Export stock movements to CSV
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>CSV file</returns>
        [HttpGet("export/stock-movements/csv")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportStockMovementsToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var csvData = await _reportingService.ExportStockMovementsToCsvAsync(filter, clientCode);
                var fileName = $"stock-movements-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export stock movements to CSV", ex);
            }
        }

        /// <summary>
        /// Export stock movements to Excel
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>Excel file</returns>
        [HttpGet("export/stock-movements/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportStockMovementsToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var excelData = await _reportingService.ExportStockMovementsToExcelAsync(filter, clientCode);
                var fileName = $"stock-movements-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export stock movements to Excel", ex);
            }
        }

        /// <summary>
        /// Export daily balances to CSV
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>CSV file</returns>
        [HttpGet("export/daily-balances/csv")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportDailyBalancesToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var csvData = await _reportingService.ExportDailyBalancesToCsvAsync(filter, clientCode);
                var fileName = $"daily-balances-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export daily balances to CSV", ex);
            }
        }

        /// <summary>
        /// Export daily balances to Excel
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>Excel file</returns>
        [HttpGet("export/daily-balances/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportDailyBalancesToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var excelData = await _reportingService.ExportDailyBalancesToExcelAsync(filter, clientCode);
                var fileName = $"daily-balances-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export daily balances to Excel", ex);
            }
        }

        /// <summary>
        /// Export sales report to CSV
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>CSV file</returns>
        [HttpGet("export/sales-report/csv")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportSalesReportToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var csvData = await _reportingService.ExportSalesReportToCsvAsync(filter, clientCode);
                var fileName = $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export sales report to CSV", ex);
            }
        }

        /// <summary>
        /// Export sales report to Excel
        /// </summary>
        /// <param name="filter">Report filter criteria</param>
        /// <returns>Excel file</returns>
        [HttpGet("export/sales-report/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportSalesReportToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var excelData = await _reportingService.ExportSalesReportToExcelAsync(filter, clientCode);
                var fileName = $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export sales report to Excel", ex);
            }
        }

        /// <summary>
        /// Export RFID usage to CSV
        /// </summary>
        /// <returns>CSV file</returns>
        [HttpGet("export/rfid-usage/csv")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportRfidUsageToCsv()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var csvData = await _reportingService.ExportRfidUsageToCsvAsync(clientCode);
                var fileName = $"rfid-usage-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export RFID usage to CSV", ex);
            }
        }

        /// <summary>
        /// Export RFID usage to Excel
        /// </summary>
        /// <returns>Excel file</returns>
        [HttpGet("export/rfid-usage/excel")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ExportRfidUsageToExcel()
        {
            try
            {
                var clientCode = GetClientCodeFromToken();
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest(new { success = false, message = "Client code not found in token" });

                var excelData = await _reportingService.ExportRfidUsageToExcelAsync(clientCode);
                var fileName = $"rfid-usage-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return ErrorResponse(500, "Failed to export RFID usage to Excel", ex);
            }
        }

        #endregion
    }
}
