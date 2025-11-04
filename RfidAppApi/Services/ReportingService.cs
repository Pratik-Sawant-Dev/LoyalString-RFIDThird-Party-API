using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Comprehensive reporting service for stock tracking, sales reporting, and daily balances
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly IClientService _clientService;

        public ReportingService(IClientService clientService)
        {
            _clientService = clientService;
        }

        #region Stock Movement Methods

        public async Task<StockMovementDto> CreateStockMovementAsync(CreateStockMovementDto movementDto, string clientCode, int? userId = null)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get product details
            var product = await context.ProductDetails
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == movementDto.ProductId);

            if (product == null)
                throw new ArgumentException($"Product with ID {movementDto.ProductId} not found");

            // If userId is provided, validate access to the product's branch and counter
            if (userId.HasValue)
            {
                // This will be handled by the controller using AccessControlService
                // The validation should be done before calling this method
            }

            var movement = new StockMovement
            {
                ClientCode = clientCode,
                ProductId = movementDto.ProductId,
                RfidCode = movementDto.RfidCode,
                MovementType = movementDto.MovementType,
                Quantity = movementDto.Quantity,
                UnitPrice = movementDto.UnitPrice,
                TotalAmount = movementDto.TotalAmount,
                BranchId = product.BranchId,
                CounterId = product.CounterId,
                CategoryId = product.CategoryId,
                ReferenceNumber = movementDto.ReferenceNumber,
                ReferenceType = movementDto.ReferenceType,
                Remarks = movementDto.Remarks,
                MovementDate = movementDto.MovementDate ?? DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            context.StockMovements.Add(movement);
            await context.SaveChangesAsync();

            return await MapToStockMovementDtoAsync(movement, context);
        }

        public async Task<List<StockMovementDto>> CreateBulkStockMovementsAsync(BulkStockMovementDto bulkDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);
            var results = new List<StockMovementDto>();

            foreach (var movementDto in bulkDto.Movements)
            {
                try
                {
                    var result = await CreateStockMovementAsync(movementDto, clientCode);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other movements
                    Console.WriteLine($"Error creating stock movement: {ex.Message}");
                }
            }

            return results;
        }

        public async Task<List<StockMovementDto>> GetStockMovementsAsync(ReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.Branch)
                .Include(sm => sm.Counter)
                .Include(sm => sm.Category)
                .Where(sm => sm.ClientCode == clientCode && sm.IsActive);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= filter.EndDate.Value);

            if (filter.BranchId.HasValue)
                query = query.Where(sm => sm.BranchId == filter.BranchId.Value);

            if (filter.CounterId.HasValue)
                query = query.Where(sm => sm.CounterId == filter.CounterId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(sm => sm.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.MovementType))
                query = query.Where(sm => sm.MovementType == filter.MovementType);

            if (!string.IsNullOrWhiteSpace(filter.RfidCode))
                query = query.Where(sm => sm.RfidCode == filter.RfidCode);

            if (!string.IsNullOrWhiteSpace(filter.ItemCode))
                query = query.Where(sm => sm.Product != null && sm.Product.ItemCode == filter.ItemCode);

            var movements = await query
                .OrderByDescending(sm => sm.MovementDate)
                .ThenByDescending(sm => sm.CreatedOn)
                .ToListAsync();

            return await MapToStockMovementDtoListAsync(movements, context);
        }

        public async Task<StockMovementDto?> GetStockMovementByIdAsync(int movementId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var movement = await context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.Branch)
                .Include(sm => sm.Counter)
                .Include(sm => sm.Category)
                .FirstOrDefaultAsync(sm => sm.Id == movementId && sm.ClientCode == clientCode && sm.IsActive);

            return movement != null ? await MapToStockMovementDtoAsync(movement, context) : null;
        }

        #endregion

        #region Daily Stock Balance Methods

        public async Task<DailyStockBalanceDto> GetDailyStockBalanceAsync(int productId, DateTime date, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var balance = await context.DailyStockBalances
                .Include(dsb => dsb.Product)
                .Include(dsb => dsb.Branch)
                .Include(dsb => dsb.Counter)
                .Include(dsb => dsb.Category)
                .FirstOrDefaultAsync(dsb => dsb.ProductId == productId && 
                                           dsb.BalanceDate.Date == date.Date && 
                                           dsb.ClientCode == clientCode && 
                                           dsb.IsActive);

            return balance != null ? await MapToDailyStockBalanceDtoAsync(balance, context) : 
                   await CalculateDailyStockBalanceAsync(productId, date, clientCode);
        }

        public async Task<List<DailyStockBalanceDto>> GetDailyStockBalancesAsync(ReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.DailyStockBalances
                .Include(dsb => dsb.Product)
                .Include(dsb => dsb.Branch)
                .Include(dsb => dsb.Counter)
                .Include(dsb => dsb.Category)
                .Where(dsb => dsb.ClientCode == clientCode && dsb.IsActive);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(dsb => dsb.BalanceDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(dsb => dsb.BalanceDate <= filter.EndDate.Value);

            if (filter.BranchId.HasValue)
                query = query.Where(dsb => dsb.BranchId == filter.BranchId.Value);

            if (filter.CounterId.HasValue)
                query = query.Where(dsb => dsb.CounterId == filter.CounterId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(dsb => dsb.CategoryId == filter.CategoryId.Value);

            var balances = await query
                .OrderByDescending(dsb => dsb.BalanceDate)
                .ThenBy(dsb => dsb.Product != null ? dsb.Product.ItemCode : "")
                .ToListAsync();

            return await MapToDailyStockBalanceDtoListAsync(balances, context);
        }

        public async Task<DailyStockBalanceDto> CalculateDailyStockBalanceAsync(int productId, DateTime date, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get product details
            var product = await context.ProductDetails
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found");

            // Get previous day's closing balance
            var previousDate = date.AddDays(-1);
            var previousBalance = await context.DailyStockBalances
                .FirstOrDefaultAsync(dsb => dsb.ProductId == productId && 
                                           dsb.BalanceDate.Date == previousDate.Date && 
                                           dsb.ClientCode == clientCode && 
                                           dsb.IsActive);

            // Get movements for the current date
            var movements = await context.StockMovements
                .Where(sm => sm.ProductId == productId && 
                            sm.MovementDate.Date == date.Date && 
                            sm.ClientCode == clientCode && 
                            sm.IsActive)
                .ToListAsync();

            // Calculate quantities
            var openingQuantity = previousBalance?.ClosingQuantity ?? 0;
            var addedQuantity = movements.Where(m => m.MovementType == "Addition").Sum(m => m.Quantity);
            var soldQuantity = movements.Where(m => m.MovementType == "Sale").Sum(m => m.Quantity);
            var returnedQuantity = movements.Where(m => m.MovementType == "Return").Sum(m => m.Quantity);
            var transferredInQuantity = movements.Where(m => m.MovementType == "TransferIn").Sum(m => m.Quantity);
            var transferredOutQuantity = movements.Where(m => m.MovementType == "TransferOut").Sum(m => m.Quantity);

            var closingQuantity = openingQuantity + addedQuantity - soldQuantity + returnedQuantity + 
                                 transferredInQuantity - transferredOutQuantity;

            // Calculate values
            var openingValue = previousBalance?.ClosingValue ?? 0;
            var addedValue = movements.Where(m => m.MovementType == "Addition").Sum(m => m.TotalAmount ?? 0);
            var soldValue = movements.Where(m => m.MovementType == "Sale").Sum(m => m.TotalAmount ?? 0);
            var returnedValue = movements.Where(m => m.MovementType == "Return").Sum(m => m.TotalAmount ?? 0);

            var closingValue = openingValue + addedValue - soldValue + returnedValue;

            // Create or update daily balance
            var balance = await context.DailyStockBalances
                .FirstOrDefaultAsync(dsb => dsb.ProductId == productId && 
                                           dsb.BalanceDate.Date == date.Date && 
                                           dsb.ClientCode == clientCode);

            if (balance == null)
            {
                // Get RFID code from ProductRfidAssignment
                var rfidAssignment = await context.ProductRfidAssignments
                    .Where(pra => pra.ProductId == productId && pra.IsActive)
                    .Select(pra => pra.RFIDCode)
                    .FirstOrDefaultAsync();

                balance = new DailyStockBalance
                {
                    ClientCode = clientCode,
                    ProductId = productId,
                    RfidCode = rfidAssignment,
                    BranchId = product.BranchId,
                    CounterId = product.CounterId,
                    CategoryId = product.CategoryId,
                    BalanceDate = date.Date,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };
                context.DailyStockBalances.Add(balance);
            }

            // Update balance values
            balance.OpeningQuantity = openingQuantity;
            balance.ClosingQuantity = closingQuantity;
            balance.AddedQuantity = addedQuantity;
            balance.SoldQuantity = soldQuantity;
            balance.ReturnedQuantity = returnedQuantity;
            balance.TransferredInQuantity = transferredInQuantity;
            balance.TransferredOutQuantity = transferredOutQuantity;
            balance.OpeningValue = openingValue;
            balance.ClosingValue = closingValue;
            balance.AddedValue = addedValue;
            balance.SoldValue = soldValue;
            balance.ReturnedValue = returnedValue;
            balance.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return await MapToDailyStockBalanceDtoAsync(balance, context);
        }

        public async Task<List<DailyStockBalanceDto>> CalculateDailyStockBalancesAsync(DateTime date, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get all active products
            var products = await context.ProductDetails
                .Where(p => p.ClientCode == clientCode && p.Status == "Active")
                .Select(p => p.Id)
                .ToListAsync();

            var results = new List<DailyStockBalanceDto>();

            foreach (var productId in products)
            {
                try
                {
                    var balance = await CalculateDailyStockBalanceAsync(productId, date, clientCode);
                    results.Add(balance);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other products
                    Console.WriteLine($"Error calculating balance for product {productId}: {ex.Message}");
                }
            }

            return results;
        }

        #endregion

        #region Sales Report Methods

        public async Task<List<SalesReportDto>> GetSalesReportAsync(ReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.Invoices
                .Include(i => i.Product)
                .ThenInclude(p => p.Branch)
                .Include(i => i.Product)
                .ThenInclude(p => p.Counter)
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Where(i => i.ClientCode == clientCode && 
                           i.InvoiceType == "Sale" && 
                           i.IsActive);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(i => i.SoldOn >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(i => i.SoldOn <= filter.EndDate.Value);

            if (filter.BranchId.HasValue)
                query = query.Where(i => i.Product != null && i.Product.BranchId == filter.BranchId.Value);

            if (filter.CounterId.HasValue)
                query = query.Where(i => i.Product != null && i.Product.CounterId == filter.CounterId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(i => i.Product != null && i.Product.CategoryId == filter.CategoryId.Value);

            var invoices = await query
                .OrderByDescending(i => i.SoldOn)
                .ToListAsync();

            // Group by date and calculate summaries
            var salesReports = invoices
                .Where(i => i.Product != null)
                .GroupBy(i => new { Date = i.SoldOn.Date, i.Product.BranchId, i.Product.CounterId, i.Product.CategoryId })
                .Select(g => new SalesReportDto
                {
                    Date = g.Key.Date,
                    BranchId = g.Key.BranchId,
                    BranchName = g.First().Product.Branch != null ? g.First().Product.Branch.BranchName : "Unknown",
                    CounterId = g.Key.CounterId,
                    CounterName = g.First().Product.Counter != null ? g.First().Product.Counter.CounterName : "Unknown",
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.First().Product.Category != null ? g.First().Product.Category.CategoryName : "Unknown",
                    TotalItemsSold = g.Count(),
                    TotalSalesAmount = g.Sum(i => i.FinalAmount),
                    TotalDiscountAmount = g.Sum(i => i.DiscountAmount),
                    NetSalesAmount = g.Sum(i => i.FinalAmount - i.DiscountAmount),
                    TotalInvoices = g.Select(i => i.InvoiceNumber).Distinct().Count(),
                    AverageTicketValue = g.Count() > 0 ? g.Sum(i => i.FinalAmount) / g.Count() : 0
                })
                .ToList();

            return salesReports;
        }

        public async Task<SalesReportDto> GetSalesReportByDateAsync(DateTime date, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date
            };

            var reports = await GetSalesReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new SalesReportDto { Date = date };
        }

        public async Task<List<SalesReportDto>> GetSalesReportByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = startDate.Date,
                EndDate = endDate.Date
            };

            return await GetSalesReportAsync(filter, clientCode);
        }

        public async Task<SalesReportDto> GetSalesReportByBranchAsync(DateTime date, int branchId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                BranchId = branchId
            };

            var reports = await GetSalesReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new SalesReportDto { Date = date, BranchId = branchId };
        }

        public async Task<SalesReportDto> GetSalesReportByCounterAsync(DateTime date, int counterId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                CounterId = counterId
            };

            var reports = await GetSalesReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new SalesReportDto { Date = date, CounterId = counterId };
        }

        public async Task<SalesReportDto> GetSalesReportByCategoryAsync(DateTime date, int categoryId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                CategoryId = categoryId
            };

            var reports = await GetSalesReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new SalesReportDto { Date = date, CategoryId = categoryId };
        }

        #endregion

        #region Stock Summary Report Methods

        public async Task<List<StockSummaryReportDto>> GetStockSummaryReportAsync(ReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.ProductDetails
                .Include(p => p.Branch)
                .Include(p => p.Counter)
                .Include(p => p.Category)
                .Where(p => p.ClientCode == clientCode);

            // Apply filters
            if (filter.BranchId.HasValue)
                query = query.Where(p => p.BranchId == filter.BranchId.Value);

            if (filter.CounterId.HasValue)
                query = query.Where(p => p.CounterId == filter.CounterId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            var products = await query.ToListAsync();

            // Get current date for summary
            var currentDate = DateTime.UtcNow.Date;

            // Group by branch, counter, category
            var summaries = products
                .GroupBy(p => new { p.BranchId, p.CounterId, p.CategoryId })
                .Select(g => new StockSummaryReportDto
                {
                    Date = currentDate,
                    BranchId = g.Key.BranchId,
                    BranchName = g.First().Branch.BranchName,
                    CounterId = g.Key.CounterId,
                    CounterName = g.First().Counter.CounterName,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.First().Category.CategoryName,
                    TotalProducts = g.Count(),
                    ActiveProducts = g.Count(p => p.Status == "Active"),
                    SoldProducts = g.Count(p => p.Status == "Sold"),
                    TotalStockValue = g.Sum(p => p.Mrp ?? 0),
                    TotalSalesValue = 0 // This would need to be calculated from sales data
                })
                .ToList();

            return summaries;
        }

        public async Task<StockSummaryReportDto> GetStockSummaryByDateAsync(DateTime date, string clientCode)
        {
            var filter = new ReportFilterDto();
            var reports = await GetStockSummaryReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new StockSummaryReportDto { Date = date };
        }

        public async Task<StockSummaryReportDto> GetStockSummaryByBranchAsync(DateTime date, int branchId, string clientCode)
        {
            var filter = new ReportFilterDto { BranchId = branchId };
            var reports = await GetStockSummaryReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new StockSummaryReportDto { Date = date, BranchId = branchId };
        }

        public async Task<StockSummaryReportDto> GetStockSummaryByCounterAsync(DateTime date, int counterId, string clientCode)
        {
            var filter = new ReportFilterDto { CounterId = counterId };
            var reports = await GetStockSummaryReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new StockSummaryReportDto { Date = date, CounterId = counterId };
        }

        public async Task<StockSummaryReportDto> GetStockSummaryByCategoryAsync(DateTime date, int categoryId, string clientCode)
        {
            var filter = new ReportFilterDto { CategoryId = categoryId };
            var reports = await GetStockSummaryReportAsync(filter, clientCode);
            return reports.FirstOrDefault() ?? new StockSummaryReportDto { Date = date, CategoryId = categoryId };
        }

        #endregion

        #region Daily Activity Report Methods

        public async Task<List<DailyActivityReportDto>> GetDailyActivityReportAsync(ReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.DailyStockBalances
                .Include(dsb => dsb.Branch)
                .Include(dsb => dsb.Counter)
                .Include(dsb => dsb.Category)
                .Where(dsb => dsb.ClientCode == clientCode && dsb.IsActive);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(dsb => dsb.BalanceDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(dsb => dsb.BalanceDate <= filter.EndDate.Value);

            if (filter.BranchId.HasValue)
                query = query.Where(dsb => dsb.BranchId == filter.BranchId.Value);

            if (filter.CounterId.HasValue)
                query = query.Where(dsb => dsb.CounterId == filter.CounterId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(dsb => dsb.CategoryId == filter.CategoryId.Value);

            var balances = await query.ToListAsync();

            // Group by date, branch, counter, category
            var activities = balances
                .GroupBy(dsb => new { dsb.BalanceDate, dsb.BranchId, dsb.CounterId, dsb.CategoryId })
                .Select(g => new DailyActivityReportDto
                {
                    Date = g.Key.BalanceDate,
                    BranchId = g.Key.BranchId,
                    BranchName = g.First().Branch.BranchName,
                    CounterId = g.Key.CounterId,
                    CounterName = g.First().Counter.CounterName,
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.First().Category.CategoryName,
                    OpeningStock = g.Sum(dsb => dsb.OpeningQuantity),
                    AddedStock = g.Sum(dsb => dsb.AddedQuantity),
                    SoldStock = g.Sum(dsb => dsb.SoldQuantity),
                    ReturnedStock = g.Sum(dsb => dsb.ReturnedQuantity),
                    TransferredInStock = g.Sum(dsb => dsb.TransferredInQuantity),
                    TransferredOutStock = g.Sum(dsb => dsb.TransferredOutQuantity),
                    ClosingStock = g.Sum(dsb => dsb.ClosingQuantity),
                    OpeningValue = g.Sum(dsb => dsb.OpeningValue ?? 0),
                    AddedValue = g.Sum(dsb => dsb.AddedValue ?? 0),
                    SoldValue = g.Sum(dsb => dsb.SoldValue ?? 0),
                    ReturnedValue = g.Sum(dsb => dsb.ReturnedValue ?? 0),
                    TransferredInValue = 0, // Would need to be calculated
                    TransferredOutValue = 0, // Would need to be calculated
                    ClosingValue = g.Sum(dsb => dsb.ClosingValue ?? 0)
                })
                .ToList();

            return activities;
        }

        public async Task<DailyActivityReportDto> GetDailyActivityByDateAsync(DateTime date, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date
            };

            var activities = await GetDailyActivityReportAsync(filter, clientCode);
            return activities.FirstOrDefault() ?? new DailyActivityReportDto { Date = date };
        }

        public async Task<DailyActivityReportDto> GetDailyActivityByBranchAsync(DateTime date, int branchId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                BranchId = branchId
            };

            var activities = await GetDailyActivityReportAsync(filter, clientCode);
            return activities.FirstOrDefault() ?? new DailyActivityReportDto { Date = date, BranchId = branchId };
        }

        public async Task<DailyActivityReportDto> GetDailyActivityByCounterAsync(DateTime date, int counterId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                CounterId = counterId
            };

            var activities = await GetDailyActivityReportAsync(filter, clientCode);
            return activities.FirstOrDefault() ?? new DailyActivityReportDto { Date = date, CounterId = counterId };
        }

        public async Task<DailyActivityReportDto> GetDailyActivityByCategoryAsync(DateTime date, int categoryId, string clientCode)
        {
            var filter = new ReportFilterDto
            {
                StartDate = date.Date,
                EndDate = date.Date,
                CategoryId = categoryId
            };

            var activities = await GetDailyActivityReportAsync(filter, clientCode);
            return activities.FirstOrDefault() ?? new DailyActivityReportDto { Date = date, CategoryId = categoryId };
        }

        #endregion

        #region Report Summary Methods

        public async Task<ReportSummaryDto> GetReportSummaryAsync(DateTime date, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get stock movements for the date
            var movements = await context.StockMovements
                .Where(sm => sm.MovementDate.Date == date.Date && 
                            sm.ClientCode == clientCode && 
                            sm.IsActive)
                .ToListAsync();

            // Get sales for the date
            var sales = await context.Invoices
                .Where(i => i.SoldOn.Date == date.Date && 
                           i.ClientCode == clientCode && 
                           i.InvoiceType == "Sale" && 
                           i.IsActive)
                .ToListAsync();

            return new ReportSummaryDto
            {
                Date = date,
                TotalProducts = await context.ProductDetails.CountAsync(p => p.ClientCode == clientCode),
                TotalAdded = movements.Where(m => m.MovementType == "Addition").Sum(m => m.Quantity),
                TotalSold = movements.Where(m => m.MovementType == "Sale").Sum(m => m.Quantity),
                TotalReturned = movements.Where(m => m.MovementType == "Return").Sum(m => m.Quantity),
                TotalAddedValue = movements.Where(m => m.MovementType == "Addition").Sum(m => m.TotalAmount ?? 0),
                TotalSoldValue = movements.Where(m => m.MovementType == "Sale").Sum(m => m.TotalAmount ?? 0),
                TotalReturnedValue = movements.Where(m => m.MovementType == "Return").Sum(m => m.TotalAmount ?? 0),
                TotalInvoices = sales.Select(s => s.InvoiceNumber).Distinct().Count(),
                NetSalesAmount = sales.Sum(s => s.FinalAmount - s.DiscountAmount)
            };
        }

        public async Task<ReportSummaryDto> GetReportSummaryByDateRangeAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get stock movements for the date range
            var movements = await context.StockMovements
                .Where(sm => sm.MovementDate.Date >= startDate.Date && 
                            sm.MovementDate.Date <= endDate.Date && 
                            sm.ClientCode == clientCode && 
                            sm.IsActive)
                .ToListAsync();

            // Get sales for the date range
            var sales = await context.Invoices
                .Where(i => i.SoldOn.Date >= startDate.Date && 
                           i.SoldOn.Date <= endDate.Date && 
                           i.ClientCode == clientCode && 
                           i.InvoiceType == "Sale" && 
                           i.IsActive)
                .ToListAsync();

            return new ReportSummaryDto
            {
                Date = startDate,
                TotalProducts = await context.ProductDetails.CountAsync(p => p.ClientCode == clientCode),
                TotalAdded = movements.Where(m => m.MovementType == "Addition").Sum(m => m.Quantity),
                TotalSold = movements.Where(m => m.MovementType == "Sale").Sum(m => m.Quantity),
                TotalReturned = movements.Where(m => m.MovementType == "Return").Sum(m => m.Quantity),
                TotalAddedValue = movements.Where(m => m.MovementType == "Addition").Sum(m => m.TotalAmount ?? 0),
                TotalSoldValue = movements.Where(m => m.MovementType == "Sale").Sum(m => m.TotalAmount ?? 0),
                TotalReturnedValue = movements.Where(m => m.MovementType == "Return").Sum(m => m.TotalAmount ?? 0),
                TotalInvoices = sales.Select(s => s.InvoiceNumber).Distinct().Count(),
                NetSalesAmount = sales.Sum(s => s.FinalAmount - s.DiscountAmount)
            };
        }

        public async Task<ReportSummaryDto> GetReportSummaryByBranchAsync(DateTime date, int branchId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get stock movements for the date and branch
            var movements = await context.StockMovements
                .Where(sm => sm.MovementDate.Date == date.Date && 
                            sm.BranchId == branchId && 
                            sm.ClientCode == clientCode && 
                            sm.IsActive)
                .ToListAsync();

            // Get sales for the date and branch
            var sales = await context.Invoices
                .Include(i => i.Product)
                .Where(i => i.SoldOn.Date == date.Date && 
                           i.Product.BranchId == branchId && 
                           i.ClientCode == clientCode && 
                           i.InvoiceType == "Sale" && 
                           i.IsActive)
                .ToListAsync();

            return new ReportSummaryDto
            {
                Date = date,
                TotalProducts = await context.ProductDetails.CountAsync(p => p.ClientCode == clientCode && p.BranchId == branchId),
                TotalAdded = movements.Where(m => m.MovementType == "Addition").Sum(m => m.Quantity),
                TotalSold = movements.Where(m => m.MovementType == "Sale").Sum(m => m.Quantity),
                TotalReturned = movements.Where(m => m.MovementType == "Return").Sum(m => m.Quantity),
                TotalAddedValue = movements.Where(m => m.MovementType == "Addition").Sum(m => m.TotalAmount ?? 0),
                TotalSoldValue = movements.Where(m => m.MovementType == "Sale").Sum(m => m.TotalAmount ?? 0),
                TotalReturnedValue = movements.Where(m => m.MovementType == "Return").Sum(m => m.TotalAmount ?? 0),
                TotalInvoices = sales.Select(s => s.InvoiceNumber).Distinct().Count(),
                NetSalesAmount = sales.Sum(s => s.FinalAmount - s.DiscountAmount)
            };
        }

        public async Task<ReportSummaryDto> GetReportSummaryByCounterAsync(DateTime date, int counterId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Get stock movements for the date and counter
            var movements = await context.StockMovements
                .Where(sm => sm.MovementDate.Date == date.Date && 
                            sm.CounterId == counterId && 
                            sm.ClientCode == clientCode && 
                            sm.IsActive)
                .ToListAsync();

            // Get sales for the date and counter
            var sales = await context.Invoices
                .Include(i => i.Product)
                .Where(i => i.SoldOn.Date == date.Date && 
                           i.Product.CounterId == counterId && 
                           i.ClientCode == clientCode && 
                           i.InvoiceType == "Sale" && 
                           i.IsActive)
                .ToListAsync();

            return new ReportSummaryDto
            {
                Date = date,
                TotalProducts = await context.ProductDetails.CountAsync(p => p.ClientCode == clientCode && p.CounterId == counterId),
                TotalAdded = movements.Where(m => m.MovementType == "Addition").Sum(m => m.Quantity),
                TotalSold = movements.Where(m => m.MovementType == "Sale").Sum(m => m.Quantity),
                TotalReturned = movements.Where(m => m.MovementType == "Return").Sum(m => m.Quantity),
                TotalAddedValue = movements.Where(m => m.MovementType == "Addition").Sum(m => m.TotalAmount ?? 0),
                TotalSoldValue = movements.Where(m => m.MovementType == "Sale").Sum(m => m.TotalAmount ?? 0),
                TotalReturnedValue = movements.Where(m => m.MovementType == "Return").Sum(m => m.TotalAmount ?? 0),
                TotalInvoices = sales.Select(s => s.InvoiceNumber).Distinct().Count(),
                NetSalesAmount = sales.Sum(s => s.FinalAmount - s.DiscountAmount)
            };
        }

        #endregion

        #region Stock Tracking Methods

        public async Task<int> GetCurrentStockAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == productId && p.ClientCode == clientCode);

            if (product == null) return 0;

            // Get the latest daily balance
            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingQuantity ?? 0;
        }

        public async Task<int> GetCurrentStockByBranchAsync(int productId, int branchId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.BranchId == branchId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingQuantity ?? 0;
        }

        public async Task<int> GetCurrentStockByCounterAsync(int productId, int counterId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.CounterId == counterId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingQuantity ?? 0;
        }

        public async Task<int> GetCurrentStockByCategoryAsync(int categoryId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalances = await context.DailyStockBalances
                .Include(dsb => dsb.Product)
                .Where(dsb => dsb.Product.CategoryId == categoryId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .GroupBy(dsb => dsb.ProductId)
                .Select(g => g.OrderByDescending(dsb => dsb.BalanceDate).First())
                .ToListAsync();

            return latestBalances.Sum(b => b.ClosingQuantity);
        }

        public async Task<decimal> GetCurrentStockValueAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingValue ?? 0;
        }

        public async Task<decimal> GetCurrentStockValueByBranchAsync(int productId, int branchId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.BranchId == branchId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingValue ?? 0;
        }

        public async Task<decimal> GetCurrentStockValueByCounterAsync(int productId, int counterId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalance = await context.DailyStockBalances
                .Where(dsb => dsb.ProductId == productId && 
                             dsb.CounterId == counterId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .OrderByDescending(dsb => dsb.BalanceDate)
                .FirstOrDefaultAsync();

            return latestBalance?.ClosingValue ?? 0;
        }

        public async Task<decimal> GetCurrentStockValueByCategoryAsync(int categoryId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var latestBalances = await context.DailyStockBalances
                .Include(dsb => dsb.Product)
                .Where(dsb => dsb.Product.CategoryId == categoryId && 
                             dsb.ClientCode == clientCode && 
                             dsb.IsActive)
                .GroupBy(dsb => dsb.ProductId)
                .Select(g => g.OrderByDescending(dsb => dsb.BalanceDate).First())
                .ToListAsync();

            return latestBalances.Sum(b => b.ClosingValue ?? 0);
        }

        #endregion

        #region Utility Methods

        public async Task<bool> ProcessDailyStockBalancesAsync(DateTime date, string clientCode)
        {
            try
            {
                await CalculateDailyStockBalancesAsync(date, clientCode);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing daily stock balances: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessAllDailyStockBalancesAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            try
            {
                var currentDate = startDate.Date;
                while (currentDate <= endDate.Date)
                {
                    await ProcessDailyStockBalancesAsync(currentDate, clientCode);
                    currentDate = currentDate.AddDays(1);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing daily stock balances: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecalculateStockBalancesAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            try
            {
                using var context = await _clientService.GetClientDbContextAsync(clientCode);

                // Delete existing balances for the date range
                var existingBalances = await context.DailyStockBalances
                    .Where(dsb => dsb.BalanceDate >= startDate.Date && 
                                 dsb.BalanceDate <= endDate.Date && 
                                 dsb.ClientCode == clientCode)
                    .ToListAsync();

                context.DailyStockBalances.RemoveRange(existingBalances);
                await context.SaveChangesAsync();

                // Recalculate balances
                await ProcessAllDailyStockBalancesAsync(startDate, endDate, clientCode);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recalculating stock balances: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<StockMovementDto> MapToStockMovementDtoAsync(StockMovement movement, ClientDbContext context)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                ClientCode = movement.ClientCode ?? "",
                ProductId = movement.ProductId,
                RfidCode = movement.RfidCode,
                MovementType = movement.MovementType,
                Quantity = movement.Quantity,
                UnitPrice = movement.UnitPrice,
                TotalAmount = movement.TotalAmount,
                BranchId = movement.BranchId,
                BranchName = movement.Branch?.BranchName ?? "Unknown",
                CounterId = movement.CounterId,
                CounterName = movement.Counter?.CounterName ?? "Unknown",
                CategoryId = movement.CategoryId,
                CategoryName = movement.Category?.CategoryName ?? "Unknown",
                ReferenceNumber = movement.ReferenceNumber,
                ReferenceType = movement.ReferenceType,
                Remarks = movement.Remarks,
                MovementDate = movement.MovementDate,
                CreatedOn = movement.CreatedOn
            };
        }

        private async Task<List<StockMovementDto>> MapToStockMovementDtoListAsync(List<StockMovement> movements, ClientDbContext context)
        {
            var dtos = new List<StockMovementDto>();
            foreach (var movement in movements)
            {
                dtos.Add(await MapToStockMovementDtoAsync(movement, context));
            }
            return dtos;
        }

        private async Task<DailyStockBalanceDto> MapToDailyStockBalanceDtoAsync(DailyStockBalance balance, ClientDbContext context)
        {
            return new DailyStockBalanceDto
            {
                Id = balance.Id,
                ClientCode = balance.ClientCode ?? "",
                ProductId = balance.ProductId,
                RfidCode = balance.RfidCode,
                ItemCode = balance.Product?.ItemCode ?? "Unknown",
                ProductName = balance.Product?.Product?.ProductName ?? "Unknown",
                BranchId = balance.BranchId,
                BranchName = balance.Branch?.BranchName ?? "Unknown",
                CounterId = balance.CounterId,
                CounterName = balance.Counter?.CounterName ?? "Unknown",
                CategoryId = balance.CategoryId,
                CategoryName = balance.Category?.CategoryName ?? "Unknown",
                BalanceDate = balance.BalanceDate,
                OpeningQuantity = balance.OpeningQuantity,
                ClosingQuantity = balance.ClosingQuantity,
                AddedQuantity = balance.AddedQuantity,
                SoldQuantity = balance.SoldQuantity,
                ReturnedQuantity = balance.ReturnedQuantity,
                TransferredInQuantity = balance.TransferredInQuantity,
                TransferredOutQuantity = balance.TransferredOutQuantity,
                OpeningValue = balance.OpeningValue,
                ClosingValue = balance.ClosingValue,
                AddedValue = balance.AddedValue,
                SoldValue = balance.SoldValue,
                ReturnedValue = balance.ReturnedValue,
                CreatedOn = balance.CreatedOn
            };
        }

        private async Task<List<DailyStockBalanceDto>> MapToDailyStockBalanceDtoListAsync(List<DailyStockBalance> balances, ClientDbContext context)
        {
            var dtos = new List<DailyStockBalanceDto>();
            foreach (var balance in balances)
            {
                dtos.Add(await MapToDailyStockBalanceDtoAsync(balance, context));
            }
            return dtos;
        }

        #endregion

        #region RFID Usage Report Methods

        public async Task<RfidUsageReportDto> GetRfidUsageReportAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            var usedRfidTags = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .Select(pra => pra.RFIDCode)
                .Distinct()
                .CountAsync();

            var unusedRfidTags = totalRfidTags - usedRfidTags;
            var usagePercentage = totalRfidTags > 0 ? (decimal)usedRfidTags / totalRfidTags * 100 : 0;
            var unusedPercentage = totalRfidTags > 0 ? (decimal)unusedRfidTags / totalRfidTags * 100 : 0;

            var usedRfidDetails = await GetUsedRfidTagsAsync(clientCode);
            var unusedRfidDetails = await GetUnusedRfidTagsAsync(clientCode);

            return new RfidUsageReportDto
            {
                ClientCode = clientCode,
                TotalRfidTags = totalRfidTags,
                UsedRfidTags = usedRfidTags,
                UnusedRfidTags = unusedRfidTags,
                UsagePercentage = Math.Round(usagePercentage, 2),
                UnusedPercentage = Math.Round(unusedPercentage, 2),
                ReportDate = DateTime.UtcNow,
                UsedRfidDetails = usedRfidDetails,
                UnusedRfidDetails = unusedRfidDetails
            };
        }

        public async Task<RfidUsageReportDto> GetRfidUsageReportByDateAsync(DateTime date, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive && r.CreatedOn <= date)
                .CountAsync();

            var usedRfidTags = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode && pra.AssignedOn <= date)
                .Select(pra => pra.RFIDCode)
                .Distinct()
                .CountAsync();

            var unusedRfidTags = totalRfidTags - usedRfidTags;
            var usagePercentage = totalRfidTags > 0 ? (decimal)usedRfidTags / totalRfidTags * 100 : 0;
            var unusedPercentage = totalRfidTags > 0 ? (decimal)unusedRfidTags / totalRfidTags * 100 : 0;

            var usedRfidDetails = await GetUsedRfidTagsAsync(clientCode);
            var unusedRfidDetails = await GetUnusedRfidTagsAsync(clientCode);

            return new RfidUsageReportDto
            {
                ClientCode = clientCode,
                TotalRfidTags = totalRfidTags,
                UsedRfidTags = usedRfidTags,
                UnusedRfidTags = unusedRfidTags,
                UsagePercentage = Math.Round(usagePercentage, 2),
                UnusedPercentage = Math.Round(unusedPercentage, 2),
                ReportDate = date,
                UsedRfidDetails = usedRfidDetails,
                UnusedRfidDetails = unusedRfidDetails
            };
        }

        public async Task<List<RfidUsageDetailDto>> GetUsedRfidTagsAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var usedRfids = await context.ProductRfidAssignments
                .Include(pra => pra.Product)
                .Include(pra => pra.Product.Category)
                .Include(pra => pra.Product.Branch)
                .Include(pra => pra.Product.Counter)
                .Include(pra => pra.Rfid)
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .Select(pra => new RfidUsageDetailDto
                {
                    RfidCode = pra.RFIDCode,
                    EpcValue = pra.Rfid != null ? pra.Rfid.EPCValue : "",
                    IsUsed = true,
                    ProductId = pra.ProductId,
                    ItemCode = pra.Product != null ? pra.Product.ItemCode : "",
                    ProductName = pra.Product != null && pra.Product.Product != null ? pra.Product.Product.ProductName : "",
                    CategoryName = pra.Product != null && pra.Product.Category != null ? pra.Product.Category.CategoryName : "",
                    BranchName = pra.Product != null && pra.Product.Branch != null ? pra.Product.Branch.BranchName : "",
                    CounterName = pra.Product != null && pra.Product.Counter != null ? pra.Product.Counter.CounterName : "",
                    AssignedOn = pra.AssignedOn,
                    UnassignedOn = pra.UnassignedOn,
                    IsActive = pra.IsActive,
                    CreatedOn = pra.Rfid != null ? pra.Rfid.CreatedOn : DateTime.UtcNow
                })
                .ToListAsync();

            return usedRfids;
        }

        public async Task<List<RfidUsageDetailDto>> GetUnusedRfidTagsAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var usedRfidCodes = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .Select(pra => pra.RFIDCode)
                .Distinct()
                .ToListAsync();

            var unusedRfids = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive && !usedRfidCodes.Contains(r.RFIDCode))
                .Select(r => new RfidUsageDetailDto
                {
                    RfidCode = r.RFIDCode,
                    EpcValue = r.EPCValue,
                    IsUsed = false,
                    ProductId = null,
                    ItemCode = null,
                    ProductName = null,
                    CategoryName = null,
                    BranchName = null,
                    CounterName = null,
                    AssignedOn = null,
                    UnassignedOn = null,
                    IsActive = r.IsActive,
                    CreatedOn = r.CreatedOn
                })
                .ToListAsync();

            return unusedRfids;
        }

        public async Task<List<RfidUsageDetailDto>> GetRfidTagsByStatusAsync(bool isUsed, string clientCode)
        {
            if (isUsed)
                return await GetUsedRfidTagsAsync(clientCode);
            else
                return await GetUnusedRfidTagsAsync(clientCode);
        }

        public async Task<List<RfidUsageByCategoryDto>> GetRfidUsageByCategoryAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var categoryUsage = await context.ProductRfidAssignments
                .Include(pra => pra.Product)
                .Include(pra => pra.Product.Category)
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .GroupBy(pra => new { pra.Product.CategoryId, pra.Product.Category.CategoryName })
                .Select(g => new RfidUsageByCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    UsedRfidTags = g.Count(),
                    TotalRfidTags = 0, // Will be calculated separately
                    UnusedRfidTags = 0, // Will be calculated separately
                    UsagePercentage = 0 // Will be calculated separately
                })
                .ToListAsync();

            // Get total RFID tags for each category
            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            foreach (var category in categoryUsage)
            {
                category.TotalRfidTags = totalRfidTags;
                category.UnusedRfidTags = totalRfidTags - category.UsedRfidTags;
                category.UsagePercentage = totalRfidTags > 0 ? Math.Round((decimal)category.UsedRfidTags / totalRfidTags * 100, 2) : 0;
            }

            return categoryUsage;
        }

        public async Task<List<RfidUsageByBranchDto>> GetRfidUsageByBranchAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var branchUsage = await context.ProductRfidAssignments
                .Include(pra => pra.Product)
                .Include(pra => pra.Product.Branch)
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .GroupBy(pra => new { pra.Product.BranchId, pra.Product.Branch.BranchName })
                .Select(g => new RfidUsageByBranchDto
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.BranchName,
                    UsedRfidTags = g.Count(),
                    TotalRfidTags = 0, // Will be calculated separately
                    UnusedRfidTags = 0, // Will be calculated separately
                    UsagePercentage = 0 // Will be calculated separately
                })
                .ToListAsync();

            // Get total RFID tags
            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            foreach (var branch in branchUsage)
            {
                branch.TotalRfidTags = totalRfidTags;
                branch.UnusedRfidTags = totalRfidTags - branch.UsedRfidTags;
                branch.UsagePercentage = totalRfidTags > 0 ? Math.Round((decimal)branch.UsedRfidTags / totalRfidTags * 100, 2) : 0;
            }

            return branchUsage;
        }

        public async Task<List<RfidUsageByCounterDto>> GetRfidUsageByCounterAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var counterUsage = await context.ProductRfidAssignments
                .Include(pra => pra.Product)
                .Include(pra => pra.Product.Counter)
                .Include(pra => pra.Product.Counter.Branch)
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .GroupBy(pra => new { pra.Product.CounterId, pra.Product.Counter.CounterName, pra.Product.Counter.BranchId, pra.Product.Counter.Branch.BranchName })
                .Select(g => new RfidUsageByCounterDto
                {
                    CounterId = g.Key.CounterId,
                    CounterName = g.Key.CounterName,
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.BranchName,
                    UsedRfidTags = g.Count(),
                    TotalRfidTags = 0, // Will be calculated separately
                    UnusedRfidTags = 0, // Will be calculated separately
                    UsagePercentage = 0 // Will be calculated separately
                })
                .ToListAsync();

            // Get total RFID tags
            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            foreach (var counter in counterUsage)
            {
                counter.TotalRfidTags = totalRfidTags;
                counter.UnusedRfidTags = totalRfidTags - counter.UsedRfidTags;
                counter.UsagePercentage = totalRfidTags > 0 ? Math.Round((decimal)counter.UsedRfidTags / totalRfidTags * 100, 2) : 0;
            }

            return counterUsage;
        }

        public async Task<RfidUsageByCategoryDto> GetRfidUsageByCategoryIdAsync(int categoryId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var category = await context.CategoryMasters
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
                throw new ArgumentException($"Category with ID {categoryId} not found");

            var usedRfidTags = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode && pra.Product.CategoryId == categoryId)
                .CountAsync();

            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            var unusedRfidTags = totalRfidTags - usedRfidTags;
            var usagePercentage = totalRfidTags > 0 ? Math.Round((decimal)usedRfidTags / totalRfidTags * 100, 2) : 0;

            return new RfidUsageByCategoryDto
            {
                CategoryId = categoryId,
                CategoryName = category?.CategoryName ?? "Unknown",
                TotalRfidTags = totalRfidTags,
                UsedRfidTags = usedRfidTags,
                UnusedRfidTags = unusedRfidTags,
                UsagePercentage = usagePercentage
            };
        }

        public async Task<RfidUsageByBranchDto> GetRfidUsageByBranchIdAsync(int branchId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var branch = await context.BranchMasters
                .FirstOrDefaultAsync(b => b.BranchId == branchId && b.ClientCode == clientCode);

            if (branch == null)
                throw new ArgumentException($"Branch with ID {branchId} not found");

            var usedRfidTags = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode && pra.Product.BranchId == branchId)
                .CountAsync();

            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            var unusedRfidTags = totalRfidTags - usedRfidTags;
            var usagePercentage = totalRfidTags > 0 ? Math.Round((decimal)usedRfidTags / totalRfidTags * 100, 2) : 0;

            return new RfidUsageByBranchDto
            {
                BranchId = branchId,
                BranchName = branch?.BranchName ?? "Unknown",
                TotalRfidTags = totalRfidTags,
                UsedRfidTags = usedRfidTags,
                UnusedRfidTags = unusedRfidTags,
                UsagePercentage = usagePercentage
            };
        }

        public async Task<RfidUsageByCounterDto> GetRfidUsageByCounterIdAsync(int counterId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var counter = await context.CounterMasters
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.CounterId == counterId && c.ClientCode == clientCode);

            if (counter == null)
                throw new ArgumentException($"Counter with ID {counterId} not found");

            var usedRfidTags = await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode && pra.Product.CounterId == counterId)
                .CountAsync();

            var totalRfidTags = await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();

            var unusedRfidTags = totalRfidTags - usedRfidTags;
            var usagePercentage = totalRfidTags > 0 ? Math.Round((decimal)usedRfidTags / totalRfidTags * 100, 2) : 0;

            return new RfidUsageByCounterDto
            {
                CounterId = counterId,
                CounterName = counter?.CounterName ?? "Unknown",
                BranchId = counter?.BranchId ?? 0,
                BranchName = counter?.Branch?.BranchName ?? "Unknown",
                TotalRfidTags = totalRfidTags,
                UsedRfidTags = usedRfidTags,
                UnusedRfidTags = unusedRfidTags,
                UsagePercentage = usagePercentage
            };
        }

        public async Task<int> GetTotalRfidTagsCountAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);
            return await context.Rfids
                .Where(r => r.ClientCode == clientCode && r.IsActive)
                .CountAsync();
        }

        public async Task<int> GetUsedRfidTagsCountAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);
            return await context.ProductRfidAssignments
                .Where(pra => pra.IsActive && pra.Product.ClientCode == clientCode)
                .Select(pra => pra.RFIDCode)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetUnusedRfidTagsCountAsync(string clientCode)
        {
            var total = await GetTotalRfidTagsCountAsync(clientCode);
            var used = await GetUsedRfidTagsCountAsync(clientCode);
            return total - used;
        }

        public async Task<decimal> GetRfidUsagePercentageAsync(string clientCode)
        {
            var total = await GetTotalRfidTagsCountAsync(clientCode);
            if (total == 0) return 0;

            var used = await GetUsedRfidTagsCountAsync(clientCode);
            return Math.Round((decimal)used / total * 100, 2);
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportStockMovementsToCsvAsync(ReportFilterDto filter, string clientCode)
        {
            var movements = await GetStockMovementsAsync(filter, clientCode);
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Client Code,Product ID,RFID Code,Movement Type,Quantity,Unit Price,Total Amount,Branch Name,Counter Name,Category Name,Reference Number,Reference Type,Remarks,Movement Date,Created On");
            
            foreach (var movement in movements)
            {
                csv.AppendLine($"{movement.Id},{movement.ClientCode},{movement.ProductId},{movement.RfidCode},{movement.MovementType},{movement.Quantity},{movement.UnitPrice},{movement.TotalAmount},{movement.BranchName},{movement.CounterName},{movement.CategoryName},{movement.ReferenceNumber},{movement.ReferenceType},{movement.Remarks},{movement.MovementDate:yyyy-MM-dd HH:mm:ss},{movement.CreatedOn:yyyy-MM-dd HH:mm:ss}");
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportStockMovementsToExcelAsync(ReportFilterDto filter, string clientCode)
        {
            var movements = await GetStockMovementsAsync(filter, clientCode);
            
            // For now, return CSV format - in a real implementation, you'd use EPPlus or similar
            return await ExportStockMovementsToCsvAsync(filter, clientCode);
        }

        public async Task<byte[]> ExportDailyBalancesToCsvAsync(ReportFilterDto filter, string clientCode)
        {
            var balances = await GetDailyStockBalancesAsync(filter, clientCode);
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Client Code,Product ID,RFID Code,Item Code,Product Name,Branch Name,Counter Name,Category Name,Balance Date,Opening Quantity,Closing Quantity,Added Quantity,Sold Quantity,Returned Quantity,Transferred In,Transferred Out,Opening Value,Closing Value,Added Value,Sold Value,Returned Value,Created On");
            
            foreach (var balance in balances)
            {
                csv.AppendLine($"{balance.Id},{balance.ClientCode},{balance.ProductId},{balance.RfidCode},{balance.ItemCode},{balance.ProductName},{balance.BranchName},{balance.CounterName},{balance.CategoryName},{balance.BalanceDate:yyyy-MM-dd},{balance.OpeningQuantity},{balance.ClosingQuantity},{balance.AddedQuantity},{balance.SoldQuantity},{balance.ReturnedQuantity},{balance.TransferredInQuantity},{balance.TransferredOutQuantity},{balance.OpeningValue},{balance.ClosingValue},{balance.AddedValue},{balance.SoldValue},{balance.ReturnedValue},{balance.CreatedOn:yyyy-MM-dd HH:mm:ss}");
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportDailyBalancesToExcelAsync(ReportFilterDto filter, string clientCode)
        {
            // For now, return CSV format - in a real implementation, you'd use EPPlus or similar
            return await ExportDailyBalancesToCsvAsync(filter, clientCode);
        }

        public async Task<byte[]> ExportSalesReportToCsvAsync(ReportFilterDto filter, string clientCode)
        {
            var salesReports = await GetSalesReportAsync(filter, clientCode);
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Branch Name,Counter Name,Category Name,Total Items Sold,Total Sales Amount,Total Discount Amount,Net Sales Amount,Total Invoices,Average Ticket Value");
            
            foreach (var report in salesReports)
            {
                csv.AppendLine($"{report.Date:yyyy-MM-dd},{report.BranchName},{report.CounterName},{report.CategoryName},{report.TotalItemsSold},{report.TotalSalesAmount},{report.TotalDiscountAmount},{report.NetSalesAmount},{report.TotalInvoices},{report.AverageTicketValue}");
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportSalesReportToExcelAsync(ReportFilterDto filter, string clientCode)
        {
            // For now, return CSV format - in a real implementation, you'd use EPPlus or similar
            return await ExportSalesReportToCsvAsync(filter, clientCode);
        }

        public async Task<byte[]> ExportRfidUsageToCsvAsync(string clientCode)
        {
            var report = await GetRfidUsageReportAsync(clientCode);
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Client Code,Total RFID Tags,Used RFID Tags,Unused RFID Tags,Usage Percentage,Unused Percentage,Report Date");
            csv.AppendLine($"{report.ClientCode},{report.TotalRfidTags},{report.UsedRfidTags},{report.UnusedRfidTags},{report.UsagePercentage},{report.UnusedPercentage},{report.ReportDate:yyyy-MM-dd HH:mm:ss}");
            
            csv.AppendLine();
            csv.AppendLine("Used RFID Details:");
            csv.AppendLine("RFID Code,EPC Value,Product ID,Item Code,Product Name,Category Name,Branch Name,Counter Name,Assigned On,Unassigned On,Is Active,Created On");
            
            foreach (var detail in report.UsedRfidDetails)
            {
                csv.AppendLine($"{detail.RfidCode},{detail.EpcValue},{detail.ProductId},{detail.ItemCode},{detail.ProductName},{detail.CategoryName},{detail.BranchName},{detail.CounterName},{detail.AssignedOn:yyyy-MM-dd HH:mm:ss},{detail.UnassignedOn:yyyy-MM-dd HH:mm:ss},{detail.IsActive},{detail.CreatedOn:yyyy-MM-dd HH:mm:ss}");
            }
            
            csv.AppendLine();
            csv.AppendLine("Unused RFID Details:");
            csv.AppendLine("RFID Code,EPC Value,Is Active,Created On");
            
            foreach (var detail in report.UnusedRfidDetails)
            {
                csv.AppendLine($"{detail.RfidCode},{detail.EpcValue},{detail.IsActive},{detail.CreatedOn:yyyy-MM-dd HH:mm:ss}");
            }
            
            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportRfidUsageToExcelAsync(string clientCode)
        {
            // For now, return CSV format - in a real implementation, you'd use EPPlus or similar
            return await ExportRfidUsageToCsvAsync(clientCode);
        }

        #endregion
    }
}
