using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using RfidAppApi.Extensions;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for comprehensive invoice management functionality
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IReportingService _reportingService;
        private readonly IAccessControlService _accessControlService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceService invoiceService,
            IReportingService reportingService,
            IAccessControlService accessControlService,
            ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _reportingService = reportingService;
            _accessControlService = accessControlService;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Create a new invoice
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<InvoiceResponseDto>> CreateInvoice([FromBody] CreateInvoiceDto createDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                // Check if user can access the product's branch and counter
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                
                if (userId > 0)
                {
                    // Get product details to check branch and counter
                    var product = await _invoiceService.GetProductDetailsAsync(createDto.ProductId, clientCode);
                    if (product != null)
                    {
                        // Check if user can access the product's branch and counter
                        var canAccess = await _accessControlService.CanAccessBranchAndCounterAsync(userId, product.BranchId, product.CounterId);
                        if (!canAccess)
                        {
                            return BadRequest(new
                        {
                            success = false,
                            message = $"Access denied. You don't have permission to access branch ID {product.BranchId}."
                        });
                        }
                    }
                }

                // Validate required fields
                if (createDto.ProductId <= 0)
                    return BadRequest("ProductId is required and must be greater than 0");

                if (createDto.SellingPrice <= 0)
                    return BadRequest("Selling price must be greater than 0");

                if (createDto.FinalAmount <= 0)
                    return BadRequest("Final amount must be greater than 0");

                // Validate discount logic
                if (createDto.DiscountAmount > createDto.SellingPrice)
                    return BadRequest("Discount amount cannot be greater than selling price");

                // Validate GST percentage
                if (createDto.GstPercentage < 0 || createDto.GstPercentage > 100)
                    return BadRequest("GST percentage must be between 0 and 100");

                // Calculate expected final amount based on GST application
                var amountBeforeGst = createDto.SellingPrice - createDto.DiscountAmount;
                var expectedFinalAmount = createDto.IsGstApplied 
                    ? amountBeforeGst + Math.Round(amountBeforeGst * (createDto.GstPercentage / 100), 2)
                    : amountBeforeGst;

                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                {
                    var billType = createDto.IsGstApplied ? "Pakka Bill (with GST)" : "Kaccha Bill (without GST)";
                    return BadRequest($"Final amount should be {expectedFinalAmount} for {billType}");
                }

                var result = await _invoiceService.CreateInvoiceAsync(createDto, clientCode);
                
                // Create stock movement for the sale
                try
                {
                    var stockMovementDto = new CreateStockMovementDto
                    {
                        ProductId = createDto.ProductId,
                        RfidCode = createDto.RfidCode,
                        MovementType = "Sale",
                        Quantity = 1,
                        UnitPrice = createDto.SellingPrice,
                        TotalAmount = createDto.FinalAmount,
                        ReferenceNumber = result.InvoiceNumber,
                        ReferenceType = "Invoice",
                        Remarks = $"Invoice: {result.InvoiceNumber} - {createDto.CustomerName}",
                        MovementDate = createDto.SoldOn ?? DateTime.UtcNow
                    };

                    await _reportingService.CreateStockMovementAsync(stockMovementDto, clientCode, userId);
                    _logger.LogInformation("Stock movement created for invoice: {InvoiceNumber}", result.InvoiceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create stock movement for invoice: {InvoiceNumber}", result.InvoiceNumber);
                    // Don't fail the invoice creation if stock movement fails
                }

                return CreatedAtAction(nameof(GetInvoiceById), new { invoiceId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        [HttpGet("{invoiceId}")]
        public async Task<ActionResult<InvoiceResponseDto>> GetInvoiceById(int invoiceId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var invoice = await _invoiceService.GetInvoiceAsync(invoiceId, clientCode);
                if (invoice == null)
                    return NotFound($"Invoice with ID {invoiceId} not found");

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice by ID: {InvoiceId}", invoiceId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all invoices with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedInvoiceResponseDto>> GetAllInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? invoiceType = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? sortBy = "CreatedOn",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);

                // Apply filters
                var filteredInvoices = allInvoices.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    filteredInvoices = filteredInvoices.Where(i =>
                        i.InvoiceNumber.ToLower().Contains(searchLower) ||
                        (i.CustomerName != null && i.CustomerName.ToLower().Contains(searchLower)) ||
                        (i.CustomerPhone != null && i.CustomerPhone.Contains(searchTerm)) ||
                        i.ProductName.ToLower().Contains(searchLower) ||
                        (i.RfidCode != null && i.RfidCode.ToLower().Contains(searchLower))
                    );
                }

                if (!string.IsNullOrEmpty(invoiceType))
                {
                    filteredInvoices = filteredInvoices.Where(i => i.InvoiceType == invoiceType);
                }

                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    filteredInvoices = filteredInvoices.Where(i => i.PaymentMethod == paymentMethod);
                }

                if (startDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn <= endDate.Value);
                }

                if (minAmount.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(i => i.FinalAmount >= minAmount.Value);
                }

                if (maxAmount.HasValue)
                {
                    filteredInvoices = filteredInvoices.Where(i => i.FinalAmount <= maxAmount.Value);
                }

                // Apply sorting
                filteredInvoices = sortBy.ToLower() switch
                {
                    "invoicenumber" => sortOrder.ToLower() == "desc" 
                        ? filteredInvoices.OrderByDescending(i => i.InvoiceNumber)
                        : filteredInvoices.OrderBy(i => i.InvoiceNumber),
                    "customer" => sortOrder.ToLower() == "desc"
                        ? filteredInvoices.OrderByDescending(i => i.CustomerName)
                        : filteredInvoices.OrderBy(i => i.CustomerName),
                    "amount" => sortOrder.ToLower() == "desc"
                        ? filteredInvoices.OrderByDescending(i => i.FinalAmount)
                        : filteredInvoices.OrderBy(i => i.FinalAmount),
                    "soldon" => sortOrder.ToLower() == "desc"
                        ? filteredInvoices.OrderByDescending(i => i.SoldOn)
                        : filteredInvoices.OrderBy(i => i.SoldOn),
                    _ => sortOrder.ToLower() == "desc"
                        ? filteredInvoices.OrderByDescending(i => i.CreatedOn)
                        : filteredInvoices.OrderBy(i => i.CreatedOn)
                };

                var totalCount = filteredInvoices.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var pagedInvoices = filteredInvoices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PaginatedInvoiceResponseDto
                {
                    Invoices = pagedInvoices,
                    Pagination = new PaginationInfoDto
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing invoice
        /// </summary>
        [HttpPut("{invoiceId}")]
        public async Task<ActionResult<InvoiceResponseDto>> UpdateInvoice(int invoiceId, [FromBody] UpdateInvoiceDto updateDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                // Validate discount logic if both amounts are provided
                if (updateDto.SellingPrice.HasValue && updateDto.DiscountAmount.HasValue)
                {
                    if (updateDto.DiscountAmount.Value > updateDto.SellingPrice.Value)
                        return BadRequest("Discount amount cannot be greater than selling price");
                }

                var result = await _invoiceService.UpdateInvoiceAsync(invoiceId, updateDto, clientCode);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice: {InvoiceId}", invoiceId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete an invoice (soft delete)
        /// </summary>
        [HttpDelete("{invoiceId}")]
        public async Task<ActionResult<object>> DeleteInvoice(int invoiceId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var success = await _invoiceService.DeleteInvoiceAsync(invoiceId, clientCode);
                if (!success)
                    return NotFound($"Invoice with ID {invoiceId} not found");

                return Ok(new { success = true, message = $"Invoice {invoiceId} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice: {InvoiceId}", invoiceId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Advanced Query Operations

        /// <summary>
        /// Get invoices by date range
        /// </summary>
        [HttpGet("by-date-range")]
        public async Task<ActionResult<List<InvoiceResponseDto>>> GetInvoicesByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (startDate > endDate)
                    return BadRequest("Start date cannot be after end date");

                var invoices = await _invoiceService.GetInvoicesByDateRangeAsync(startDate, endDate, clientCode);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by date range");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoices by product ID
        /// </summary>
        [HttpGet("by-product/{productId}")]
        public async Task<ActionResult<List<InvoiceResponseDto>>> GetInvoicesByProduct(int productId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (productId <= 0)
                    return BadRequest("Product ID must be greater than 0");

                var invoices = await _invoiceService.GetInvoicesByProductAsync(productId, clientCode);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by product: {ProductId}", productId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoices by customer name (partial match)
        /// </summary>
        [HttpGet("by-customer")]
        public async Task<ActionResult<List<InvoiceResponseDto>>> GetInvoicesByCustomer([FromQuery] string customerName)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (string.IsNullOrWhiteSpace(customerName))
                    return BadRequest("Customer name is required");

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);
                var filteredInvoices = allInvoices
                    .Where(i => i.CustomerName != null && 
                               i.CustomerName.ToLower().Contains(customerName.ToLower()))
                    .ToList();

                return Ok(filteredInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by customer: {CustomerName}", customerName);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoices by payment method
        /// </summary>
        [HttpGet("by-payment-method")]
        public async Task<ActionResult<List<InvoiceResponseDto>>> GetInvoicesByPaymentMethod([FromQuery] string paymentMethod)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (string.IsNullOrWhiteSpace(paymentMethod))
                    return BadRequest("Payment method is required");

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);
                var filteredInvoices = allInvoices
                    .Where(i => i.PaymentMethod != null && 
                               i.PaymentMethod.ToLower() == paymentMethod.ToLower())
                    .ToList();

                return Ok(filteredInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by payment method: {PaymentMethod}", paymentMethod);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Analytics and Statistics

        /// <summary>
        /// Get comprehensive invoice statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<InvoiceStatisticsDto>> GetInvoiceStatistics()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var statistics = await _invoiceService.GetInvoiceStatisticsAsync(clientCode);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice statistics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get revenue analytics by date range
        /// </summary>
        [HttpGet("revenue-analytics")]
        public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenueAnalytics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? groupBy = "day") // day, week, month
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (startDate > endDate)
                    return BadRequest("Start date cannot be after end date");

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);
                var filteredInvoices = allInvoices
                    .Where(i => i.SoldOn >= startDate && i.SoldOn <= endDate)
                    .ToList();

                var analytics = new RevenueAnalyticsDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    GroupBy = groupBy,
                    TotalRevenue = filteredInvoices.Sum(i => i.FinalAmount),
                    TotalInvoices = filteredInvoices.Count,
                    AverageTicketValue = filteredInvoices.Any() ? filteredInvoices.Average(i => i.FinalAmount) : 0,
                    RevenueByPeriod = new List<RevenuePeriodDto>()
                };

                // Group by period
                var groupedData = groupBy.ToLower() switch
                {
                    "week" => filteredInvoices.GroupBy(i => new { Year = i.SoldOn.Year, Week = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(i.SoldOn, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) }).Cast<IGrouping<object, InvoiceResponseDto>>(),
                    "month" => filteredInvoices.GroupBy(i => new { i.SoldOn.Year, i.SoldOn.Month }).Cast<IGrouping<object, InvoiceResponseDto>>(),
                    _ => filteredInvoices.GroupBy(i => i.SoldOn.Date).Cast<IGrouping<object, InvoiceResponseDto>>()
                };

                foreach (var group in groupedData)
                {
                    var periodKey = group.Key.ToString();
                    var revenue = group.Sum(i => i.FinalAmount);
                    var count = group.Count();

                    analytics.RevenueByPeriod.Add(new RevenuePeriodDto
                    {
                        Period = periodKey,
                        Revenue = revenue,
                        InvoiceCount = count,
                        AverageTicketValue = count > 0 ? revenue / count : 0
                    });
                }

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get top performing products by revenue
        /// </summary>
        [HttpGet("top-products")]
        public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (limit <= 0 || limit > 100)
                    limit = 10;

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);
                var filteredInvoices = allInvoices.AsQueryable();

                if (startDate.HasValue)
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn >= startDate.Value);

                if (endDate.HasValue)
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn <= endDate.Value);

                var topProducts = filteredInvoices
                    .GroupBy(i => new { i.ProductId, i.ProductName })
                    .Select(g => new TopProductDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.ProductName,
                        TotalRevenue = g.Sum(i => i.FinalAmount),
                        TotalInvoices = g.Count(),
                        AverageTicketValue = g.Average(i => i.FinalAmount),
                        LastSoldOn = g.Max(i => i.SoldOn)
                    })
                    .OrderByDescending(p => p.TotalRevenue)
                    .Take(limit)
                    .ToList();

                return Ok(topProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Create multiple invoices in bulk
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkInvoiceResponseDto>> CreateBulkInvoices([FromBody] BulkCreateInvoiceDto bulkDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                if (bulkDto.Invoices == null || !bulkDto.Invoices.Any())
                    return BadRequest("At least one invoice is required");

                if (bulkDto.Invoices.Count > 100)
                    return BadRequest("Maximum 100 invoices can be created at once");

                var result = new BulkInvoiceResponseDto
                {
                    TotalInvoices = bulkDto.Invoices.Count,
                    SuccessfullyCreated = 0,
                    Failed = 0,
                    CreatedInvoices = new List<InvoiceResponseDto>(),
                    Errors = new List<string>()
                };

                foreach (var invoiceDto in bulkDto.Invoices)
                {
                    try
                    {
                        // Validate invoice data
                        if (invoiceDto.ProductId <= 0)
                        {
                            result.Failed++;
                            result.Errors.Add($"Invoice {result.Failed}: ProductId is required and must be greater than 0");
                            continue;
                        }

                        if (invoiceDto.SellingPrice <= 0)
                        {
                            result.Failed++;
                            result.Errors.Add($"Invoice {result.Failed}: Selling price must be greater than 0");
                            continue;
                        }

                        var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoiceDto, clientCode);
                        result.CreatedInvoices.Add(createdInvoice);
                        result.SuccessfullyCreated++;

                        // Create stock movement
                        try
                        {
                            var stockMovementDto = new CreateStockMovementDto
                            {
                                ProductId = invoiceDto.ProductId,
                                RfidCode = invoiceDto.RfidCode,
                                MovementType = "Sale",
                                Quantity = 1,
                                UnitPrice = invoiceDto.SellingPrice,
                                TotalAmount = invoiceDto.FinalAmount,
                                ReferenceNumber = createdInvoice.InvoiceNumber,
                                ReferenceType = "Invoice",
                                Remarks = $"Bulk Invoice: {createdInvoice.InvoiceNumber} - {invoiceDto.CustomerName}",
                                MovementDate = invoiceDto.SoldOn ?? DateTime.UtcNow
                            };

                            await _reportingService.CreateStockMovementAsync(stockMovementDto, clientCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to create stock movement for bulk invoice: {InvoiceNumber}", createdInvoice.InvoiceNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        result.Errors.Add($"Invoice {result.Failed}: {ex.Message}");
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk invoices");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Export Operations

        /// <summary>
        /// Export invoices to CSV format
        /// </summary>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportInvoicesToCsv(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? invoiceType = null)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var allInvoices = await _invoiceService.GetAllInvoicesAsync(clientCode);
                var filteredInvoices = allInvoices.AsQueryable();

                if (startDate.HasValue)
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn >= startDate.Value);

                if (endDate.HasValue)
                    filteredInvoices = filteredInvoices.Where(i => i.SoldOn <= endDate.Value);

                if (!string.IsNullOrEmpty(invoiceType))
                    filteredInvoices = filteredInvoices.Where(i => i.InvoiceType == invoiceType);

                var invoices = filteredInvoices.ToList();

                var csvContent = GenerateCsvContent(invoices);
                var fileName = $"invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to CSV");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Enhanced Invoice Operations

        /// <summary>
        /// Create invoice with multiple payment methods
        /// </summary>
        [HttpPost("with-multiple-payments")]
        public async Task<ActionResult<InvoiceWithPaymentsResponseDto>> CreateInvoiceWithMultiplePayments([FromBody] CreateInvoiceWithMultiplePaymentsDto createDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                // Validate required fields
                if (createDto.ProductId <= 0)
                    return BadRequest("ProductId is required and must be greater than 0");

                if (createDto.SellingPrice <= 0)
                    return BadRequest("Selling price must be greater than 0");

                if (createDto.FinalAmount <= 0)
                    return BadRequest("Final amount must be greater than 0");

                // Validate discount logic
                if (createDto.DiscountAmount > createDto.SellingPrice)
                    return BadRequest("Discount amount cannot be greater than selling price");

                // Validate GST percentage
                if (createDto.GstPercentage < 0 || createDto.GstPercentage > 100)
                    return BadRequest("GST percentage must be between 0 and 100");

                // Calculate expected final amount based on GST application
                var amountBeforeGst = createDto.SellingPrice - createDto.DiscountAmount;
                var expectedFinalAmount = createDto.IsGstApplied 
                    ? amountBeforeGst + Math.Round(amountBeforeGst * (createDto.GstPercentage / 100), 2)
                    : amountBeforeGst;

                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                {
                    var billType = createDto.IsGstApplied ? "Pakka Bill (with GST)" : "Kaccha Bill (without GST)";
                    return BadRequest($"Final amount should be {expectedFinalAmount} for {billType}");
                }

                // Validate payment methods
                if (createDto.PaymentMethods == null || !createDto.PaymentMethods.Any())
                    return BadRequest("At least one payment method is required");

                var totalPaymentAmount = createDto.PaymentMethods.Sum(p => p.Amount);
                if (Math.Abs(totalPaymentAmount - createDto.FinalAmount) > 0.01m)
                    return BadRequest($"Total payment amount ({totalPaymentAmount}) must equal final amount ({createDto.FinalAmount})");

                var result = await _invoiceService.CreateInvoiceWithMultiplePaymentsAsync(createDto, clientCode);
                
                // Create stock movement for the sale
                try
                {
                    var stockMovementDto = new CreateStockMovementDto
                    {
                        ProductId = createDto.ProductId,
                        RfidCode = createDto.RfidCode,
                        MovementType = "Sale",
                        Quantity = 1,
                        UnitPrice = createDto.SellingPrice,
                        TotalAmount = createDto.FinalAmount,
                        ReferenceNumber = result.InvoiceNumber,
                        ReferenceType = "Invoice",
                        Remarks = $"Invoice: {result.InvoiceNumber} - {createDto.CustomerName}",
                        MovementDate = createDto.SoldOn ?? DateTime.UtcNow
                    };

                    await _reportingService.CreateStockMovementAsync(stockMovementDto, clientCode);
                    _logger.LogInformation("Stock movement created for invoice: {InvoiceNumber}", result.InvoiceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create stock movement for invoice: {InvoiceNumber}", result.InvoiceNumber);
                    // Don't fail the invoice creation if stock movement fails
                }

                return CreatedAtAction(nameof(GetInvoiceWithPayments), new { invoiceId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice with multiple payments");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create invoice by item code
        /// </summary>
        [HttpPost("by-item-code")]
        public async Task<ActionResult<InvoiceWithPaymentsResponseDto>> CreateInvoiceByItemCode([FromBody] CreateInvoiceByItemCodeDto createDto)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(createDto.ItemCode))
                    return BadRequest("ItemCode is required");

                if (createDto.SellingPrice <= 0)
                    return BadRequest("Selling price must be greater than 0");

                if (createDto.FinalAmount <= 0)
                    return BadRequest("Final amount must be greater than 0");

                // Validate discount logic
                if (createDto.DiscountAmount > createDto.SellingPrice)
                    return BadRequest("Discount amount cannot be greater than selling price");

                // Validate GST percentage
                if (createDto.GstPercentage < 0 || createDto.GstPercentage > 100)
                    return BadRequest("GST percentage must be between 0 and 100");

                // Calculate expected final amount based on GST application
                var amountBeforeGst = createDto.SellingPrice - createDto.DiscountAmount;
                var expectedFinalAmount = createDto.IsGstApplied 
                    ? amountBeforeGst + Math.Round(amountBeforeGst * (createDto.GstPercentage / 100), 2)
                    : amountBeforeGst;

                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                {
                    var billType = createDto.IsGstApplied ? "Pakka Bill (with GST)" : "Kaccha Bill (without GST)";
                    return BadRequest($"Final amount should be {expectedFinalAmount} for {billType}");
                }

                // Validate payment methods
                if (createDto.PaymentMethods == null || !createDto.PaymentMethods.Any())
                    return BadRequest("At least one payment method is required");

                var totalPaymentAmount = createDto.PaymentMethods.Sum(p => p.Amount);
                if (Math.Abs(totalPaymentAmount - createDto.FinalAmount) > 0.01m)
                    return BadRequest($"Total payment amount ({totalPaymentAmount}) must equal final amount ({createDto.FinalAmount})");

                var result = await _invoiceService.CreateInvoiceByItemCodeAsync(createDto, clientCode);
                
                // Create stock movement for the sale
                try
                {
                    var stockMovementDto = new CreateStockMovementDto
                    {
                        ProductId = result.ProductId,
                        RfidCode = createDto.RfidCode,
                        MovementType = "Sale",
                        Quantity = 1,
                        UnitPrice = createDto.SellingPrice,
                        TotalAmount = createDto.FinalAmount,
                        ReferenceNumber = result.InvoiceNumber,
                        ReferenceType = "Invoice",
                        Remarks = $"Invoice: {result.InvoiceNumber} - {createDto.CustomerName}",
                        MovementDate = createDto.SoldOn ?? DateTime.UtcNow
                    };

                    await _reportingService.CreateStockMovementAsync(stockMovementDto, clientCode);
                    _logger.LogInformation("Stock movement created for invoice: {InvoiceNumber}", result.InvoiceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create stock movement for invoice: {InvoiceNumber}", result.InvoiceNumber);
                    // Don't fail the invoice creation if stock movement fails
                }

                return CreatedAtAction(nameof(GetInvoiceWithPayments), new { invoiceId = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice by item code");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get invoice with payment details
        /// </summary>
        [HttpGet("{invoiceId}/with-payments")]
        public async Task<ActionResult<InvoiceWithPaymentsResponseDto>> GetInvoiceWithPayments(int invoiceId)
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var invoice = await _invoiceService.GetInvoiceWithPaymentsAsync(invoiceId, clientCode);
                if (invoice == null)
                    return NotFound($"Invoice with ID {invoiceId} not found");

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice with payments by ID: {InvoiceId}", invoiceId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all invoices with payment details
        /// </summary>
        [HttpGet("with-payments")]
        public async Task<ActionResult<List<InvoiceWithPaymentsResponseDto>>> GetAllInvoicesWithPayments()
        {
            try
            {
                var clientCode = User.FindFirst("ClientCode")?.Value;
                if (string.IsNullOrEmpty(clientCode))
                    return BadRequest("Client code not found in token");

                var invoices = await _invoiceService.GetAllInvoicesWithPaymentsAsync(clientCode);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices with payments");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private string GenerateCsvContent(List<InvoiceResponseDto> invoices)
        {
            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("Invoice Number,Product Name,RFID Code,Customer Name,Customer Phone,Selling Price,Discount Amount,Final Amount,Invoice Type,Payment Method,Sold On,Remarks,Created On");
            
            // Data rows
            foreach (var invoice in invoices)
            {
                csv.AppendLine($"\"{invoice.InvoiceNumber}\",\"{invoice.ProductName}\",\"{invoice.RfidCode ?? ""}\",\"{invoice.CustomerName ?? ""}\",\"{invoice.CustomerPhone ?? ""}\",{invoice.SellingPrice},{invoice.DiscountAmount},{invoice.FinalAmount},\"{invoice.InvoiceType}\",\"{invoice.PaymentMethod ?? ""}\",\"{invoice.SoldOn:yyyy-MM-dd HH:mm:ss}\",\"{invoice.Remarks ?? ""}\",\"{invoice.CreatedOn:yyyy-MM-dd HH:mm:ss}\"");
            }
            
            return csv.ToString();
        }

        #endregion
    }

    #region Additional DTOs

    /// <summary>
    /// DTO for paginated invoice response
    /// </summary>
    public class PaginatedInvoiceResponseDto
    {
        public List<InvoiceResponseDto> Invoices { get; set; } = new List<InvoiceResponseDto>();
        public PaginationInfoDto Pagination { get; set; } = new PaginationInfoDto();
    }

    /// <summary>
    /// DTO for pagination information
    /// </summary>
    public class PaginationInfoDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for bulk invoice creation
    /// </summary>
    public class BulkCreateInvoiceDto
    {
        public List<CreateInvoiceDto> Invoices { get; set; } = new List<CreateInvoiceDto>();
    }

    /// <summary>
    /// DTO for bulk invoice response
    /// </summary>
    public class BulkInvoiceResponseDto
    {
        public int TotalInvoices { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int Failed { get; set; }
        public List<InvoiceResponseDto> CreatedInvoices { get; set; } = new List<InvoiceResponseDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO for revenue analytics
    /// </summary>
    public class RevenueAnalyticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GroupBy { get; set; } = "day";
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public decimal AverageTicketValue { get; set; }
        public List<RevenuePeriodDto> RevenueByPeriod { get; set; } = new List<RevenuePeriodDto>();
    }

    /// <summary>
    /// DTO for revenue period
    /// </summary>
    public class RevenuePeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AverageTicketValue { get; set; }
    }

    /// <summary>
    /// DTO for top performing products
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public decimal AverageTicketValue { get; set; }
        public DateTime LastSoldOn { get; set; }
    }

    #endregion
}
