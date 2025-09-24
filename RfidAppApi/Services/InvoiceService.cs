using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IClientService _clientService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IClientService clientService, ILogger<InvoiceService> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createDto, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                // Validate product exists and is available for sale
                var product = await context.ProductDetails
                    .FirstOrDefaultAsync(p => p.Id == createDto.ProductId && p.Status == "Active");
                
                if (product == null)
                    throw new ArgumentException($"Product with ID {createDto.ProductId} not found or inactive");

                // Check if product is already sold (has an active invoice)
                var existingInvoice = await context.Invoices
                    .FirstOrDefaultAsync(i => i.ProductId == createDto.ProductId && i.IsActive);
                
                if (existingInvoice != null)
                    throw new ArgumentException($"Product with ID {createDto.ProductId} (Item Code: {product.ItemCode}) has already been sold. Invoice Number: {existingInvoice.InvoiceNumber}");

                // Calculate GST amounts
                var (amountBeforeGst, gstAmount, totalAmountWithGst) = CalculateGstAmounts(
                    createDto.SellingPrice, createDto.DiscountAmount, createDto.GstPercentage, createDto.IsGstApplied);

                // Validate final amount based on GST application
                var expectedFinalAmount = createDto.IsGstApplied ? totalAmountWithGst : amountBeforeGst;
                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                    throw new ArgumentException($"Final amount should be {expectedFinalAmount} (GST {(createDto.IsGstApplied ? "applied" : "not applied")})");

                var invoice = new Invoice
                {
                    ClientCode = clientCode,
                    InvoiceNumber = await GenerateInvoiceNumberAsync(context, clientCode),
                    ProductId = createDto.ProductId,
                    RfidCode = createDto.RfidCode,
                    SellingPrice = createDto.SellingPrice,
                    DiscountAmount = createDto.DiscountAmount,
                    FinalAmount = createDto.FinalAmount,
                    IsGstApplied = createDto.IsGstApplied,
                    GstPercentage = createDto.GstPercentage,
                    GstAmount = gstAmount,
                    AmountBeforeGst = amountBeforeGst,
                    TotalAmountWithGst = totalAmountWithGst,
                    InvoiceType = createDto.InvoiceType ?? "Sale",
                    CustomerName = createDto.CustomerName,
                    CustomerPhone = createDto.CustomerPhone,
                    CustomerAddress = createDto.CustomerAddress,
                    PaymentMethod = createDto.PaymentMethod,
                    PaymentReference = createDto.PaymentReference,
                    SoldOn = createDto.SoldOn ?? DateTime.UtcNow,
                    Remarks = createDto.Remarks,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                _logger.LogInformation("Invoice created successfully: {InvoiceNumber} for client: {ClientCode}", 
                    invoice.InvoiceNumber, clientCode);

                return await MapToResponseDtoAsync(invoice, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for client: {ClientCode}", clientCode);
                throw;
            }
        }

        public async Task<InvoiceResponseDto?> GetInvoiceAsync(int invoiceId, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoice = await context.Invoices
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive);

                return invoice != null ? await MapToResponseDtoAsync(invoice) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceId} for client: {ClientCode}", invoiceId, clientCode);
                throw;
            }
        }

        public async Task<List<InvoiceResponseDto>> GetAllInvoicesAsync(string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Where(i => i.IsActive)
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                var result = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices for client: {ClientCode}", clientCode);
                throw;
            }
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceDto updateDto, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoice = await context.Invoices
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive);
                
                if (invoice == null)
                    throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");

                // Update properties
                if (updateDto.SellingPrice.HasValue)
                    invoice.SellingPrice = updateDto.SellingPrice.Value;
                if (updateDto.DiscountAmount.HasValue)
                    invoice.DiscountAmount = updateDto.DiscountAmount.Value;
                if (updateDto.FinalAmount.HasValue)
                    invoice.FinalAmount = updateDto.FinalAmount.Value;
                if (!string.IsNullOrEmpty(updateDto.CustomerName))
                    invoice.CustomerName = updateDto.CustomerName;
                if (!string.IsNullOrEmpty(updateDto.CustomerPhone))
                    invoice.CustomerPhone = updateDto.CustomerPhone;
                if (!string.IsNullOrEmpty(updateDto.CustomerAddress))
                    invoice.CustomerAddress = updateDto.CustomerAddress;
                if (!string.IsNullOrEmpty(updateDto.PaymentMethod))
                    invoice.PaymentMethod = updateDto.PaymentMethod;
                if (!string.IsNullOrEmpty(updateDto.PaymentReference))
                    invoice.PaymentReference = updateDto.PaymentReference;
                if (!string.IsNullOrEmpty(updateDto.Remarks))
                    invoice.Remarks = updateDto.Remarks;

                invoice.UpdatedOn = DateTime.UtcNow;

                await context.SaveChangesAsync();

                _logger.LogInformation("Invoice updated successfully: {InvoiceId} for client: {ClientCode}", 
                    invoiceId, clientCode);

                return await MapToResponseDtoAsync(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId} for client: {ClientCode}", invoiceId, clientCode);
                throw;
            }
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoice = await context.Invoices
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive);
                
                if (invoice == null)
                    return false;

                invoice.IsActive = false;
                invoice.UpdatedOn = DateTime.UtcNow;

                await context.SaveChangesAsync();

                _logger.LogInformation("Invoice deleted successfully: {InvoiceId} for client: {ClientCode}", 
                    invoiceId, clientCode);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId} for client: {ClientCode}", invoiceId, clientCode);
                throw;
            }
        }

        public async Task<List<InvoiceResponseDto>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Where(i => i.IsActive && i.SoldOn >= startDate && i.SoldOn <= endDate)
                    .OrderByDescending(i => i.SoldOn)
                    .ToListAsync();

                var result = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by date range for client: {ClientCode}", clientCode);
                throw;
            }
        }

        public async Task<List<InvoiceResponseDto>> GetInvoicesByProductAsync(int productId, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Where(i => i.IsActive && i.ProductId == productId)
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                var result = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by product {ProductId} for client: {ClientCode}", productId, clientCode);
                throw;
            }
        }

        public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var thisYear = new DateTime(today.Year, 1, 1);

                var totalInvoices = await context.Invoices.CountAsync(i => i.IsActive);
                var todayInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= today);
                var monthInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= thisMonth);
                var yearInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= thisYear);

                var totalRevenue = await context.Invoices
                    .Where(i => i.IsActive)
                    .SumAsync(i => i.FinalAmount);
                var todayRevenue = await context.Invoices
                    .Where(i => i.IsActive && i.SoldOn >= today)
                    .SumAsync(i => i.FinalAmount);
                var monthRevenue = await context.Invoices
                    .Where(i => i.IsActive && i.SoldOn >= thisMonth)
                    .SumAsync(i => i.FinalAmount);
                var yearRevenue = await context.Invoices
                    .Where(i => i.IsActive && i.SoldOn >= thisYear)
                    .SumAsync(i => i.FinalAmount);

                return new InvoiceStatisticsDto
                {
                    TotalInvoices = totalInvoices,
                    TodayInvoices = todayInvoices,
                    MonthInvoices = monthInvoices,
                    YearInvoices = yearInvoices,
                    TotalRevenue = totalRevenue,
                    TodayRevenue = todayRevenue,
                    MonthRevenue = monthRevenue,
                    YearRevenue = yearRevenue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice statistics for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get invoices by customer name (partial match)
        /// </summary>
        public async Task<List<InvoiceResponseDto>> GetInvoicesByCustomerAsync(string customerName, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Where(i => i.IsActive && !string.IsNullOrEmpty(i.CustomerName) && 
                               i.CustomerName.ToLower().Contains(customerName.ToLower()))
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                var result = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by customer for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get invoices by payment method
        /// </summary>
        public async Task<List<InvoiceResponseDto>> GetInvoicesByPaymentMethodAsync(string paymentMethod, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Where(i => i.IsActive && !string.IsNullOrEmpty(i.PaymentMethod) && 
                               i.PaymentMethod.ToLower() == paymentMethod.ToLower())
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                var result = new List<InvoiceResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices by payment method for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        public async Task<InvoiceResponseDto?> GetInvoiceByNumberAsync(string invoiceNumber, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoice = await context.Invoices
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && i.IsActive);

                return invoice != null ? await MapToResponseDtoAsync(invoice) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice by number {InvoiceNumber} for client: {ClientCode}", invoiceNumber, clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get invoice count by status
        /// </summary>
        public async Task<InvoiceCountDto> GetInvoiceCountByStatusAsync(string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var totalInvoices = await context.Invoices.CountAsync(i => i.IsActive);
                var todayInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= DateTime.Today);
                var thisWeekInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= DateTime.Today.AddDays(-7));
                var thisMonthInvoices = await context.Invoices.CountAsync(i => i.IsActive && i.SoldOn >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));

                return new InvoiceCountDto
                {
                    Total = totalInvoices,
                    Today = todayInvoices,
                    ThisWeek = thisWeekInvoices,
                    ThisMonth = thisMonthInvoices
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice count by status for client: {ClientCode}", clientCode);
                throw;
            }
        }

        private async Task<string> GenerateInvoiceNumberAsync(ClientDbContext context, string clientCode)
        {
            try
            {
                var today = DateTime.Today;
                var startOfDay = today;
                var endOfDay = today.AddDays(1);

                var lastInvoice = await context.Invoices
                    .Where(i => i.CreatedOn >= startOfDay && i.CreatedOn < endOfDay)
                    .OrderByDescending(i => i.CreatedOn)
                    .FirstOrDefaultAsync();

                if (lastInvoice == null)
                {
                    return $"{clientCode}-INV-{today:yyyyMMdd}-001";
                }

                var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Split('-').Last());
                var newNumber = lastNumber + 1;
                return $"{clientCode}-INV-{today:yyyyMMdd}-{newNumber:D3}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice number for client: {ClientCode}", clientCode);
                // Fallback to timestamp-based number
                return $"{clientCode}-INV-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        private async Task<InvoiceResponseDto> MapToResponseDtoAsync(Invoice invoice, ProductDetails? product = null)
        {
            try
            {
                if (product == null && invoice.Product != null)
                {
                    product = invoice.Product;
                }

                return new InvoiceResponseDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    ProductId = invoice.ProductId,
                    ProductName = product?.ItemCode ?? "Unknown",
                    RfidCode = invoice.RfidCode,
                    SellingPrice = invoice.SellingPrice,
                    DiscountAmount = invoice.DiscountAmount,
                    FinalAmount = invoice.FinalAmount,
                    IsGstApplied = invoice.IsGstApplied,
                    GstPercentage = invoice.GstPercentage,
                    GstAmount = invoice.GstAmount,
                    AmountBeforeGst = invoice.AmountBeforeGst,
                    TotalAmountWithGst = invoice.TotalAmountWithGst,
                    InvoiceType = invoice.InvoiceType,
                    CustomerName = invoice.CustomerName,
                    CustomerPhone = invoice.CustomerPhone,
                    CustomerAddress = invoice.CustomerAddress,
                    PaymentMethod = invoice.PaymentMethod,
                    PaymentReference = invoice.PaymentReference,
                    SoldOn = invoice.SoldOn,
                    Remarks = invoice.Remarks,
                    IsActive = invoice.IsActive,
                    CreatedOn = invoice.CreatedOn,
                    UpdatedOn = invoice.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping invoice to DTO: {InvoiceId}", invoice.Id);
                throw;
            }
        }

        private async Task<InvoiceWithPaymentsResponseDto> MapToPaymentsResponseDtoAsync(Invoice invoice, ProductDetails? product = null)
        {
            try
            {
                if (product == null && invoice.Product != null)
                {
                    product = invoice.Product;
                }

                return new InvoiceWithPaymentsResponseDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    ProductId = invoice.ProductId,
                    ProductName = product?.ItemCode ?? "Unknown",
                    RfidCode = invoice.RfidCode,
                    SellingPrice = invoice.SellingPrice,
                    DiscountAmount = invoice.DiscountAmount,
                    FinalAmount = invoice.FinalAmount,
                    IsGstApplied = invoice.IsGstApplied,
                    GstPercentage = invoice.GstPercentage,
                    GstAmount = invoice.GstAmount,
                    AmountBeforeGst = invoice.AmountBeforeGst,
                    TotalAmountWithGst = invoice.TotalAmountWithGst,
                    InvoiceType = invoice.InvoiceType,
                    CustomerName = invoice.CustomerName,
                    CustomerPhone = invoice.CustomerPhone,
                    CustomerAddress = invoice.CustomerAddress,
                    PaymentMethods = invoice.Payments?.Select(p => new PaymentMethodDto
                    {
                        PaymentMethod = p.PaymentMethod,
                        Amount = p.Amount,
                        PaymentReference = p.PaymentReference,
                        Remarks = p.Remarks
                    }).ToList() ?? new List<PaymentMethodDto>(),
                    SoldOn = invoice.SoldOn,
                    Remarks = invoice.Remarks,
                    IsActive = invoice.IsActive,
                    CreatedOn = invoice.CreatedOn,
                    UpdatedOn = invoice.UpdatedOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping invoice to payments DTO: {InvoiceId}", invoice.Id);
                throw;
            }
        }

        /// <summary>
        /// Create invoice with multiple payment methods
        /// </summary>
        public async Task<InvoiceWithPaymentsResponseDto> CreateInvoiceWithMultiplePaymentsAsync(CreateInvoiceWithMultiplePaymentsDto createDto, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                // Validate product exists and is available for sale
                var product = await context.ProductDetails
                    .FirstOrDefaultAsync(p => p.Id == createDto.ProductId && p.Status == "Active");
                
                if (product == null)
                    throw new ArgumentException($"Product with ID {createDto.ProductId} not found or inactive");

                // Check if product is already sold (has an active invoice)
                var existingInvoice = await context.Invoices
                    .FirstOrDefaultAsync(i => i.ProductId == createDto.ProductId && i.IsActive);
                
                if (existingInvoice != null)
                    throw new ArgumentException($"Product with ID {createDto.ProductId} (Item Code: {product.ItemCode}) has already been sold. Invoice Number: {existingInvoice.InvoiceNumber}");

                // Calculate GST amounts
                var (amountBeforeGst, gstAmount, totalAmountWithGst) = CalculateGstAmounts(
                    createDto.SellingPrice, createDto.DiscountAmount, createDto.GstPercentage, createDto.IsGstApplied);

                // Validate final amount based on GST application
                var expectedFinalAmount = createDto.IsGstApplied ? totalAmountWithGst : amountBeforeGst;
                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                    throw new ArgumentException($"Final amount should be {expectedFinalAmount} (GST {(createDto.IsGstApplied ? "applied" : "not applied")})");

                // Validate payment methods
                if (createDto.PaymentMethods == null || !createDto.PaymentMethods.Any())
                    throw new ArgumentException("At least one payment method is required");

                var totalPaymentAmount = createDto.PaymentMethods.Sum(p => p.Amount);
                if (Math.Abs(totalPaymentAmount - createDto.FinalAmount) > 0.01m)
                    throw new ArgumentException($"Total payment amount ({totalPaymentAmount}) must equal final amount ({createDto.FinalAmount})");

                var invoice = new Invoice
                {
                    ClientCode = clientCode,
                    InvoiceNumber = await GenerateInvoiceNumberAsync(context, clientCode),
                    ProductId = createDto.ProductId,
                    RfidCode = createDto.RfidCode,
                    SellingPrice = createDto.SellingPrice,
                    DiscountAmount = createDto.DiscountAmount,
                    FinalAmount = createDto.FinalAmount,
                    IsGstApplied = createDto.IsGstApplied,
                    GstPercentage = createDto.GstPercentage,
                    GstAmount = gstAmount,
                    AmountBeforeGst = amountBeforeGst,
                    TotalAmountWithGst = totalAmountWithGst,
                    InvoiceType = createDto.InvoiceType ?? "Sale",
                    CustomerName = createDto.CustomerName,
                    CustomerPhone = createDto.CustomerPhone,
                    CustomerAddress = createDto.CustomerAddress,
                    PaymentMethod = createDto.PaymentMethods.First().PaymentMethod, // Keep for backward compatibility
                    PaymentReference = createDto.PaymentMethods.First().PaymentReference,
                    SoldOn = createDto.SoldOn ?? DateTime.UtcNow,
                    Remarks = createDto.Remarks,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                // Create payment records
                foreach (var paymentDto in createDto.PaymentMethods)
                {
                    var payment = new InvoicePayment
                    {
                        InvoiceId = invoice.Id,
                        PaymentMethod = paymentDto.PaymentMethod,
                        Amount = paymentDto.Amount,
                        PaymentReference = paymentDto.PaymentReference,
                        Remarks = paymentDto.Remarks,
                        CreatedOn = DateTime.UtcNow
                    };
                    context.InvoicePayments.Add(payment);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Invoice with multiple payments created successfully: {InvoiceNumber} for client: {ClientCode}", 
                    invoice.InvoiceNumber, clientCode);

                return await MapToPaymentsResponseDtoAsync(invoice, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice with multiple payments for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Create invoice by item code
        /// </summary>
        public async Task<InvoiceWithPaymentsResponseDto> CreateInvoiceByItemCodeAsync(CreateInvoiceByItemCodeDto createDto, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                // Find product by item code and check availability
                var product = await context.ProductDetails
                    .FirstOrDefaultAsync(p => p.ItemCode == createDto.ItemCode && p.Status == "Active");
                
                if (product == null)
                    throw new ArgumentException($"Product with item code '{createDto.ItemCode}' not found or inactive");

                // Check if product is already sold (has an active invoice)
                var existingInvoice = await context.Invoices
                    .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.IsActive);
                
                if (existingInvoice != null)
                    throw new ArgumentException($"Product with item code '{createDto.ItemCode}' (Product ID: {product.Id}) has already been sold. Invoice Number: {existingInvoice.InvoiceNumber}");

                // Calculate GST amounts
                var (amountBeforeGst, gstAmount, totalAmountWithGst) = CalculateGstAmounts(
                    createDto.SellingPrice, createDto.DiscountAmount, createDto.GstPercentage, createDto.IsGstApplied);

                // Validate final amount based on GST application
                var expectedFinalAmount = createDto.IsGstApplied ? totalAmountWithGst : amountBeforeGst;
                if (Math.Abs(createDto.FinalAmount - expectedFinalAmount) > 0.01m)
                    throw new ArgumentException($"Final amount should be {expectedFinalAmount} (GST {(createDto.IsGstApplied ? "applied" : "not applied")})");

                // Validate payment methods
                if (createDto.PaymentMethods == null || !createDto.PaymentMethods.Any())
                    throw new ArgumentException("At least one payment method is required");

                var totalPaymentAmount = createDto.PaymentMethods.Sum(p => p.Amount);
                if (Math.Abs(totalPaymentAmount - createDto.FinalAmount) > 0.01m)
                    throw new ArgumentException($"Total payment amount ({totalPaymentAmount}) must equal final amount ({createDto.FinalAmount})");

                var invoice = new Invoice
                {
                    ClientCode = clientCode,
                    InvoiceNumber = await GenerateInvoiceNumberAsync(context, clientCode),
                    ProductId = product.Id,
                    RfidCode = createDto.RfidCode,
                    SellingPrice = createDto.SellingPrice,
                    DiscountAmount = createDto.DiscountAmount,
                    FinalAmount = createDto.FinalAmount,
                    IsGstApplied = createDto.IsGstApplied,
                    GstPercentage = createDto.GstPercentage,
                    GstAmount = gstAmount,
                    AmountBeforeGst = amountBeforeGst,
                    TotalAmountWithGst = totalAmountWithGst,
                    InvoiceType = createDto.InvoiceType ?? "Sale",
                    CustomerName = createDto.CustomerName,
                    CustomerPhone = createDto.CustomerPhone,
                    CustomerAddress = createDto.CustomerAddress,
                    PaymentMethod = createDto.PaymentMethods.First().PaymentMethod, // Keep for backward compatibility
                    PaymentReference = createDto.PaymentMethods.First().PaymentReference,
                    SoldOn = createDto.SoldOn ?? DateTime.UtcNow,
                    Remarks = createDto.Remarks,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                // Create payment records
                foreach (var paymentDto in createDto.PaymentMethods)
                {
                    var payment = new InvoicePayment
                    {
                        InvoiceId = invoice.Id,
                        PaymentMethod = paymentDto.PaymentMethod,
                        Amount = paymentDto.Amount,
                        PaymentReference = paymentDto.PaymentReference,
                        Remarks = paymentDto.Remarks,
                        CreatedOn = DateTime.UtcNow
                    };
                    context.InvoicePayments.Add(payment);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Invoice by item code created successfully: {InvoiceNumber} for client: {ClientCode}", 
                    invoice.InvoiceNumber, clientCode);

                return await MapToPaymentsResponseDtoAsync(invoice, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice by item code for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get invoice with payment details
        /// </summary>
        public async Task<InvoiceWithPaymentsResponseDto?> GetInvoiceWithPaymentsAsync(int invoiceId, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoice = await context.Invoices
                    .Include(i => i.Product)
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && i.IsActive);

                return invoice != null ? await MapToPaymentsResponseDtoAsync(invoice) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice with payments {InvoiceId} for client: {ClientCode}", invoiceId, clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get all invoices with payment details
        /// </summary>
        public async Task<List<InvoiceWithPaymentsResponseDto>> GetAllInvoicesWithPaymentsAsync(string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                var invoices = await context.Invoices
                    .Include(i => i.Product)
                    .Include(i => i.Payments)
                    .Where(i => i.IsActive)
                    .OrderByDescending(i => i.CreatedOn)
                    .ToListAsync();

                var result = new List<InvoiceWithPaymentsResponseDto>();
                foreach (var invoice in invoices)
                {
                    result.Add(await MapToPaymentsResponseDtoAsync(invoice));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices with payments for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Calculate GST amounts based on selling price and discount
        /// </summary>
        private (decimal amountBeforeGst, decimal gstAmount, decimal totalAmountWithGst) CalculateGstAmounts(
            decimal sellingPrice, decimal discountAmount, decimal gstPercentage, bool isGstApplied)
        {
            var amountBeforeGst = sellingPrice - discountAmount;
            
            if (!isGstApplied)
            {
                // Kaccha Bill - No GST
                return (amountBeforeGst, 0, amountBeforeGst);
            }
            else
            {
                // Pakka Bill - GST applied
                var gstAmount = Math.Round(amountBeforeGst * (gstPercentage / 100), 2);
                var totalAmountWithGst = amountBeforeGst + gstAmount;
                return (amountBeforeGst, gstAmount, totalAmountWithGst);
            }
        }
    }

    /// <summary>
    /// DTO for invoice count by status
    /// </summary>
    public class InvoiceCountDto
    {
        public int Total { get; set; }
        public int Today { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
    }
}
