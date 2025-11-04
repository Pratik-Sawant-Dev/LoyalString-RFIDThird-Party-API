using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for comprehensive reporting functionality
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportingController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        #region Stock Movement Endpoints

        /// <summary>
        /// Create a new stock movement
        /// </summary>
        [HttpPost("stock-movements")]
        public async Task<ActionResult<StockMovementDto>> CreateStockMovement([FromBody] CreateStockMovementDto movementDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var result = await _reportingService.CreateStockMovementAsync(movementDto, clientCode);
                return CreatedAtAction(nameof(GetStockMovementById), new { movementId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create multiple stock movements in bulk
        /// </summary>
        [HttpPost("stock-movements/bulk")]
        public async Task<ActionResult<List<StockMovementDto>>> CreateBulkStockMovements([FromBody] BulkStockMovementDto bulkDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var results = await _reportingService.CreateBulkStockMovementsAsync(bulkDto, clientCode);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements with filters
        /// </summary>
        [HttpGet("stock-movements")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovements([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movement by ID
        /// </summary>
        [HttpGet("stock-movements/{movementId}")]
        public async Task<ActionResult<StockMovementDto>> GetStockMovementById(int movementId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var movement = await _reportingService.GetStockMovementByIdAsync(movementId, clientCode);
                if (movement == null)
                    return NotFound($"Stock movement with ID {movementId} not found");

                return Ok(movement);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements by date range
        /// </summary>
        [HttpGet("stock-movements/range")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovementsByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements by product
        /// </summary>
        [HttpGet("stock-movements/product/{productId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovementsByProduct(int productId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto();
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                var productMovements = movements.Where(m => m.ProductId == productId).ToList();
                return Ok(productMovements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements by branch
        /// </summary>
        [HttpGet("stock-movements/branch/{branchId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovementsByBranch(int branchId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto { BranchId = branchId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements by counter
        /// </summary>
        [HttpGet("stock-movements/counter/{counterId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovementsByCounter(int counterId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto { CounterId = counterId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock movements by category
        /// </summary>
        [HttpGet("stock-movements/category/{categoryId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetStockMovementsByCategory(int categoryId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto { CategoryId = categoryId };
                var movements = await _reportingService.GetStockMovementsAsync(filter, clientCode);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Daily Stock Balance Endpoints

        /// <summary>
        /// Get daily stock balance for a specific product and date
        /// </summary>
        [HttpGet("daily-balances/{productId}/{date:datetime}")]
        public async Task<ActionResult<DailyStockBalanceDto>> GetDailyStockBalance(int productId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var balance = await _reportingService.GetDailyStockBalanceAsync(productId, date, clientCode);
                return Ok(balance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily stock balances with filters
        /// </summary>
        [HttpGet("daily-balances")]
        public async Task<ActionResult<List<DailyStockBalanceDto>>> GetDailyStockBalances([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var balances = await _reportingService.GetDailyStockBalancesAsync(filter, clientCode);
                return Ok(balances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate daily stock balance for a specific product and date
        /// </summary>
        [HttpPost("daily-balances/calculate/{productId}/{date:datetime}")]
        public async Task<ActionResult<DailyStockBalanceDto>> CalculateDailyStockBalance(int productId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var balance = await _reportingService.CalculateDailyStockBalanceAsync(productId, date, clientCode);
                return Ok(balance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate daily stock balances for all products on a specific date
        /// </summary>
        [HttpPost("daily-balances/calculate/{date:datetime}")]
        public async Task<ActionResult<List<DailyStockBalanceDto>>> CalculateDailyStockBalances(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var balances = await _reportingService.CalculateDailyStockBalancesAsync(date, clientCode);
                return Ok(balances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily stock balance by product and date
        /// </summary>
        [HttpGet("daily-balances/product/{productId}/{date:datetime}")]
        public async Task<ActionResult<DailyStockBalanceDto>> GetDailyStockBalanceByProduct(int productId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var balance = await _reportingService.GetDailyStockBalanceAsync(productId, date, clientCode);
                return Ok(balance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily stock balances by date range
        /// </summary>
        [HttpGet("daily-balances/range")]
        public async Task<ActionResult<List<DailyStockBalanceDto>>> GetDailyStockBalancesByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var filter = new ReportFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var balances = await _reportingService.GetDailyStockBalancesAsync(filter, clientCode);
                return Ok(balances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Sales Report Endpoints

        /// <summary>
        /// Get sales report with filters
        /// </summary>
        [HttpGet("sales")]
        public async Task<ActionResult<List<SalesReportDto>>> GetSalesReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var reports = await _reportingService.GetSalesReportAsync(filter, clientCode);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get sales report for a specific date
        /// </summary>
        [HttpGet("sales/date/{date:datetime}")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReportByDate(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetSalesReportByDateAsync(date, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get sales report for a date range
        /// </summary>
        [HttpGet("sales/range")]
        public async Task<ActionResult<List<SalesReportDto>>> GetSalesReportByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var reports = await _reportingService.GetSalesReportByDateRangeAsync(startDate, endDate, clientCode);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get sales report for a specific branch on a date
        /// </summary>
        [HttpGet("sales/branch/{branchId}/{date:datetime}")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReportByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetSalesReportByBranchAsync(date, branchId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get sales report for a specific counter on a date
        /// </summary>
        [HttpGet("sales/counter/{counterId}/{date:datetime}")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReportByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetSalesReportByCounterAsync(date, counterId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get sales report for a specific category on a date
        /// </summary>
        [HttpGet("sales/category/{categoryId}/{date:datetime}")]
        public async Task<ActionResult<SalesReportDto>> GetSalesReportByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetSalesReportByCategoryAsync(date, categoryId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Stock Summary Report Endpoints

        /// <summary>
        /// Get stock summary report with filters
        /// </summary>
        [HttpGet("stock-summary")]
        public async Task<ActionResult<List<StockSummaryReportDto>>> GetStockSummaryReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var reports = await _reportingService.GetStockSummaryReportAsync(filter, clientCode);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock summary for a specific date
        /// </summary>
        [HttpGet("stock-summary/date/{date:datetime}")]
        public async Task<ActionResult<StockSummaryReportDto>> GetStockSummaryByDate(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetStockSummaryByDateAsync(date, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock summary for a specific branch on a date
        /// </summary>
        [HttpGet("stock-summary/branch/{branchId}/{date:datetime}")]
        public async Task<ActionResult<StockSummaryReportDto>> GetStockSummaryByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetStockSummaryByBranchAsync(date, branchId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock summary for a specific counter on a date
        /// </summary>
        [HttpGet("stock-summary/counter/{counterId}/{date:datetime}")]
        public async Task<ActionResult<StockSummaryReportDto>> GetStockSummaryByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetStockSummaryByCounterAsync(date, counterId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get stock summary for a specific category on a date
        /// </summary>
        [HttpGet("stock-summary/category/{categoryId}/{date:datetime}")]
        public async Task<ActionResult<StockSummaryReportDto>> GetStockSummaryByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetStockSummaryByCategoryAsync(date, categoryId, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Daily Activity Report Endpoints

        /// <summary>
        /// Get daily activity report with filters
        /// </summary>
        [HttpGet("daily-activity")]
        public async Task<ActionResult<List<DailyActivityReportDto>>> GetDailyActivityReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var activities = await _reportingService.GetDailyActivityReportAsync(filter, clientCode);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily activity for a specific date
        /// </summary>
        [HttpGet("daily-activity/date/{date:datetime}")]
        public async Task<ActionResult<DailyActivityReportDto>> GetDailyActivityByDate(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var activity = await _reportingService.GetDailyActivityByDateAsync(date, clientCode);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily activity for a specific branch on a date
        /// </summary>
        [HttpGet("daily-activity/branch/{branchId}/{date:datetime}")]
        public async Task<ActionResult<DailyActivityReportDto>> GetDailyActivityByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var activity = await _reportingService.GetDailyActivityByBranchAsync(date, branchId, clientCode);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily activity for a specific counter on a date
        /// </summary>
        [HttpGet("daily-activity/counter/{counterId}/{date:datetime}")]
        public async Task<ActionResult<DailyActivityReportDto>> GetDailyActivityByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var activity = await _reportingService.GetDailyActivityByCounterAsync(date, counterId, clientCode);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get daily activity for a specific category on a date
        /// </summary>
        [HttpGet("daily-activity/category/{categoryId}/{date:datetime}")]
        public async Task<ActionResult<DailyActivityReportDto>> GetDailyActivityByCategory(int categoryId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var activity = await _reportingService.GetDailyActivityByCategoryAsync(date, categoryId, clientCode);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Report Summary Endpoints

        /// <summary>
        /// Get report summary for a specific date
        /// </summary>
        [HttpGet("summary/{date:datetime}")]
        public async Task<ActionResult<ReportSummaryDto>> GetReportSummary(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var summary = await _reportingService.GetReportSummaryAsync(date, clientCode);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get report summary for a date range
        /// </summary>
        [HttpGet("summary/range")]
        public async Task<ActionResult<ReportSummaryDto>> GetReportSummaryByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var summary = await _reportingService.GetReportSummaryByDateRangeAsync(startDate, endDate, clientCode);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get report summary for a specific branch on a date
        /// </summary>
        [HttpGet("summary/branch/{branchId}/{date:datetime}")]
        public async Task<ActionResult<ReportSummaryDto>> GetReportSummaryByBranch(int branchId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var summary = await _reportingService.GetReportSummaryByBranchAsync(date, branchId, clientCode);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get report summary for a specific counter on a date
        /// </summary>
        [HttpGet("summary/counter/{counterId}/{date:datetime}")]
        public async Task<ActionResult<ReportSummaryDto>> GetReportSummaryByCounter(int counterId, DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var summary = await _reportingService.GetReportSummaryByCounterAsync(date, counterId, clientCode);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Stock Tracking Endpoints

        /// <summary>
        /// Get current stock for a specific product
        /// </summary>
        [HttpGet("stock/current/{productId}")]
        public async Task<ActionResult<int>> GetCurrentStock(int productId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var stock = await _reportingService.GetCurrentStockAsync(productId, clientCode);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock for a specific product by branch
        /// </summary>
        [HttpGet("stock/current/{productId}/branch/{branchId}")]
        public async Task<ActionResult<int>> GetCurrentStockByBranch(int productId, int branchId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var stock = await _reportingService.GetCurrentStockByBranchAsync(productId, branchId, clientCode);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock for a specific product by counter
        /// </summary>
        [HttpGet("stock/current/{productId}/counter/{counterId}")]
        public async Task<ActionResult<int>> GetCurrentStockByCounter(int productId, int counterId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var stock = await _reportingService.GetCurrentStockByCounterAsync(productId, counterId, clientCode);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock for a specific category
        /// </summary>
        [HttpGet("stock/current/category/{categoryId}")]
        public async Task<ActionResult<int>> GetCurrentStockByCategory(int categoryId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var stock = await _reportingService.GetCurrentStockByCategoryAsync(categoryId, clientCode);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock value for a specific product
        /// </summary>
        [HttpGet("stock/value/{productId}")]
        public async Task<ActionResult<decimal>> GetCurrentStockValue(int productId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var value = await _reportingService.GetCurrentStockValueAsync(productId, clientCode);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock value for a specific product by branch
        /// </summary>
        [HttpGet("stock/value/{productId}/branch/{branchId}")]
        public async Task<ActionResult<decimal>> GetCurrentStockValueByBranch(int productId, int branchId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var value = await _reportingService.GetCurrentStockValueByBranchAsync(productId, branchId, clientCode);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock value for a specific product by counter
        /// </summary>
        [HttpGet("stock/value/{productId}/counter/{counterId}")]
        public async Task<ActionResult<decimal>> GetCurrentStockValueByCounter(int productId, int counterId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var value = await _reportingService.GetCurrentStockValueByCounterAsync(productId, counterId, clientCode);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current stock value for a specific category
        /// </summary>
        [HttpGet("stock/value/category/{categoryId}")]
        public async Task<ActionResult<decimal>> GetCurrentStockValueByCategory(int categoryId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var value = await _reportingService.GetCurrentStockValueByCategoryAsync(categoryId, clientCode);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Process daily stock balances for a specific date
        /// </summary>
        [HttpPost("process-balances/{date:datetime}")]
        public async Task<ActionResult<bool>> ProcessDailyStockBalances(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var result = await _reportingService.ProcessDailyStockBalancesAsync(date, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Process daily stock balances for a date range
        /// </summary>
        [HttpPost("process-balances/range")]
        public async Task<ActionResult<bool>> ProcessAllDailyStockBalances(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var result = await _reportingService.ProcessAllDailyStockBalancesAsync(startDate, endDate, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Recalculate stock balances for a date range
        /// </summary>
        [HttpPost("recalculate-balances")]
        public async Task<ActionResult<bool>> RecalculateStockBalances(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var result = await _reportingService.RecalculateStockBalancesAsync(startDate, endDate, clientCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region RFID Usage Report Endpoints

        /// <summary>
        /// Get comprehensive RFID usage report
        /// </summary>
        [HttpGet("rfid-usage")]
        public async Task<ActionResult<RfidUsageReportDto>> GetRfidUsageReport()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetRfidUsageReportAsync(clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage report for a specific date
        /// </summary>
        [HttpGet("rfid-usage/date/{date:datetime}")]
        public async Task<ActionResult<RfidUsageReportDto>> GetRfidUsageReportByDate(DateTime date)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var report = await _reportingService.GetRfidUsageReportByDateAsync(date, clientCode);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all used RFID tags
        /// </summary>
        [HttpGet("rfid-usage/used")]
        public async Task<ActionResult<List<RfidUsageDetailDto>>> GetUsedRfidTags()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var usedTags = await _reportingService.GetUsedRfidTagsAsync(clientCode);
                return Ok(usedTags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all unused RFID tags
        /// </summary>
        [HttpGet("rfid-usage/unused")]
        public async Task<ActionResult<List<RfidUsageDetailDto>>> GetUnusedRfidTags()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var unusedTags = await _reportingService.GetUnusedRfidTagsAsync(clientCode);
                return Ok(unusedTags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID tags by usage status
        /// </summary>
        [HttpGet("rfid-usage/status/{isUsed:bool}")]
        public async Task<ActionResult<List<RfidUsageDetailDto>>> GetRfidTagsByStatus(bool isUsed)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var tags = await _reportingService.GetRfidTagsByStatusAsync(isUsed, clientCode);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary by category
        /// </summary>
        [HttpGet("rfid-usage/by-category")]
        public async Task<ActionResult<List<RfidUsageByCategoryDto>>> GetRfidUsageByCategory()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var categoryUsage = await _reportingService.GetRfidUsageByCategoryAsync(clientCode);
                return Ok(categoryUsage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary by branch
        /// </summary>
        [HttpGet("rfid-usage/by-branch")]
        public async Task<ActionResult<List<RfidUsageByBranchDto>>> GetRfidUsageByBranch()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var branchUsage = await _reportingService.GetRfidUsageByBranchAsync(clientCode);
                return Ok(branchUsage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary by counter
        /// </summary>
        [HttpGet("rfid-usage/by-counter")]
        public async Task<ActionResult<List<RfidUsageByCounterDto>>> GetRfidUsageByCounter()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var counterUsage = await _reportingService.GetRfidUsageByCounterAsync(clientCode);
                return Ok(counterUsage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific category
        /// </summary>
        [HttpGet("rfid-usage/category/{categoryId}")]
        public async Task<ActionResult<RfidUsageByCategoryDto>> GetRfidUsageByCategoryId(int categoryId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var categoryUsage = await _reportingService.GetRfidUsageByCategoryIdAsync(categoryId, clientCode);
                return Ok(categoryUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific branch
        /// </summary>
        [HttpGet("rfid-usage/branch/{branchId}")]
        public async Task<ActionResult<RfidUsageByBranchDto>> GetRfidUsageByBranchId(int branchId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var branchUsage = await _reportingService.GetRfidUsageByBranchIdAsync(branchId, clientCode);
                return Ok(branchUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage summary for a specific counter
        /// </summary>
        [HttpGet("rfid-usage/counter/{counterId}")]
        public async Task<ActionResult<RfidUsageByCounterDto>> GetRfidUsageByCounterId(int counterId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var counterUsage = await _reportingService.GetRfidUsageByCounterIdAsync(counterId, clientCode);
                return Ok(counterUsage);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get total count of RFID tags
        /// </summary>
        [HttpGet("rfid-usage/count/total")]
        public async Task<ActionResult<int>> GetTotalRfidTagsCount()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var count = await _reportingService.GetTotalRfidTagsCountAsync(clientCode);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get count of used RFID tags
        /// </summary>
        [HttpGet("rfid-usage/count/used")]
        public async Task<ActionResult<int>> GetUsedRfidTagsCount()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var count = await _reportingService.GetUsedRfidTagsCountAsync(clientCode);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get count of unused RFID tags
        /// </summary>
        [HttpGet("rfid-usage/count/unused")]
        public async Task<ActionResult<int>> GetUnusedRfidTagsCount()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var count = await _reportingService.GetUnusedRfidTagsCountAsync(clientCode);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get RFID usage percentage
        /// </summary>
        [HttpGet("rfid-usage/percentage")]
        public async Task<ActionResult<decimal>> GetRfidUsagePercentage()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var percentage = await _reportingService.GetRfidUsagePercentageAsync(clientCode);
                return Ok(percentage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Export Endpoints

        /// <summary>
        /// Export stock movements to CSV
        /// </summary>
        [HttpGet("export/stock-movements/csv")]
        public async Task<ActionResult> ExportStockMovementsToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var csvData = await _reportingService.ExportStockMovementsToCsvAsync(filter, clientCode);
                var fileName = $"stock-movements-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export stock movements to Excel
        /// </summary>
        [HttpGet("export/stock-movements/excel")]
        public async Task<ActionResult> ExportStockMovementsToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var excelData = await _reportingService.ExportStockMovementsToExcelAsync(filter, clientCode);
                var fileName = $"stock-movements-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export daily balances to CSV
        /// </summary>
        [HttpGet("export/daily-balances/csv")]
        public async Task<ActionResult> ExportDailyBalancesToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var csvData = await _reportingService.ExportDailyBalancesToCsvAsync(filter, clientCode);
                var fileName = $"daily-balances-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export daily balances to Excel
        /// </summary>
        [HttpGet("export/daily-balances/excel")]
        public async Task<ActionResult> ExportDailyBalancesToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var excelData = await _reportingService.ExportDailyBalancesToExcelAsync(filter, clientCode);
                var fileName = $"daily-balances-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export sales report to CSV
        /// </summary>
        [HttpGet("export/sales-report/csv")]
        public async Task<ActionResult> ExportSalesReportToCsv([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var csvData = await _reportingService.ExportSalesReportToCsvAsync(filter, clientCode);
                var fileName = $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export sales report to Excel
        /// </summary>
        [HttpGet("export/sales-report/excel")]
        public async Task<ActionResult> ExportSalesReportToExcel([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var excelData = await _reportingService.ExportSalesReportToExcelAsync(filter, clientCode);
                var fileName = $"sales-report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export RFID usage to CSV
        /// </summary>
        [HttpGet("export/rfid-usage/csv")]
        public async Task<ActionResult> ExportRfidUsageToCsv()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var csvData = await _reportingService.ExportRfidUsageToCsvAsync(clientCode);
                var fileName = $"rfid-usage-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
                
                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export RFID usage to Excel
        /// </summary>
        [HttpGet("export/rfid-usage/excel")]
        public async Task<ActionResult> ExportRfidUsageToExcel()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var excelData = await _reportingService.ExportRfidUsageToExcelAsync(clientCode);
                var fileName = $"rfid-usage-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion
    }
}
