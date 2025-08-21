using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing stock verification sessions and reports
    /// </summary>
    public class StockVerificationService : IStockVerificationService
    {
        private readonly IClientService _clientService;

        public StockVerificationService(IClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task<StockVerificationResponseDto> CreateStockVerificationAsync(CreateStockVerificationDto request, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Validate branch, counter, and category exist by name
            var branch = await context.BranchMasters.FirstOrDefaultAsync(b => b.BranchName == request.BranchName);
            var counter = await context.CounterMasters.FirstOrDefaultAsync(c => c.CounterName == request.CounterName);
            var category = await context.CategoryMasters.FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (branch == null || counter == null || category == null)
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Invalid branch name, counter name, or category ID"
                };
            }

            var verification = new StockVerification
            {
                ClientCode = clientCode,
                VerificationSessionName = request.VerificationSessionName,
                Description = request.Description,
                VerificationDate = request.VerificationDate.Date,
                VerificationTime = request.VerificationTime,
                BranchId = branch.BranchId,
                CounterId = counter.CounterId,
                CategoryId = request.CategoryId,
                VerifiedBy = request.VerifiedBy,
                Remarks = request.Remarks,
                Status = "InProgress",
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            context.StockVerifications.Add(verification);
            await context.SaveChangesAsync();

            var response = await MapToStockVerificationDtoAsync(verification, context);
            return new StockVerificationResponseDto
            {
                Success = true,
                Message = "Stock verification session created successfully",
                Data = response
            };
        }

        public async Task<StockVerificationResponseDto> SubmitStockVerificationAsync(SubmitStockVerificationDto request, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verification = await context.StockVerifications
                .Include(sv => sv.VerificationDetails)
                .FirstOrDefaultAsync(sv => sv.Id == request.StockVerificationId && sv.ClientCode == clientCode);

            if (verification == null)
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Stock verification session not found"
                };
            }

            if (verification.Status != "InProgress")
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Cannot submit to a completed or cancelled verification session"
                };
            }

            var now = DateTime.UtcNow;
            var currentTime = now.TimeOfDay;

            // Process matched items
            foreach (var itemCode in request.MatchedItemCodes)
            {
                var product = await context.ProductDetails
                    .Include(p => p.Category)
                    .Include(p => p.Product)
                    .Include(p => p.Design)
                    .Include(p => p.Purity)
                    .Include(p => p.Branch)
                    .Include(p => p.Counter)
                    .FirstOrDefaultAsync(p => p.ItemCode == itemCode && p.ClientCode == clientCode);

                if (product != null)
                {
                    var rfidAssignment = await context.ProductRfidAssignments
                        .Where(pra => pra.ProductId == product.Id && pra.IsActive)
                        .Select(pra => pra.RFIDCode)
                        .FirstOrDefaultAsync();

                    var detail = new StockVerificationDetail
                    {
                        StockVerificationId = verification.Id,
                        ClientCode = clientCode,
                        ItemCode = itemCode,
                        RfidCode = rfidAssignment,
                        VerificationStatus = "Matched",
                        ScannedAt = now,
                        ScannedTime = currentTime,
                        ScannedBy = request.ScannedBy,
                        Remarks = request.Remarks,
                        CreatedOn = now,
                        IsActive = true
                    };

                    context.StockVerificationDetails.Add(detail);
                }
            }

            // Process unmatched items
            foreach (var itemCode in request.UnmatchedItemCodes)
            {
                var detail = new StockVerificationDetail
                {
                    StockVerificationId = verification.Id,
                    ClientCode = clientCode,
                    ItemCode = itemCode,
                    RfidCode = null,
                    VerificationStatus = "Unmatched",
                    ScannedAt = now,
                    ScannedTime = currentTime,
                    ScannedBy = request.ScannedBy,
                    Remarks = request.Remarks,
                    CreatedOn = now,
                    IsActive = true
                };

                context.StockVerificationDetails.Add(detail);
            }

            // Update verification session counts
            verification.TotalItemsScanned = request.MatchedItemCodes.Count + request.UnmatchedItemCodes.Count;
            verification.MatchedItemsCount = request.MatchedItemCodes.Count;
            verification.UnmatchedItemsCount = request.UnmatchedItemCodes.Count;
            verification.UpdatedOn = now;

            await context.SaveChangesAsync();

            // Calculate values
            await CalculateVerificationValuesAsync(verification.Id, context);

            var response = await MapToStockVerificationDtoAsync(verification, context);
            return new StockVerificationResponseDto
            {
                Success = true,
                Message = $"Stock verification submitted successfully. {verification.MatchedItemsCount} matched, {verification.UnmatchedItemsCount} unmatched items.",
                Data = response
            };
        }

        public async Task<StockVerificationResponseDto> GetStockVerificationByIdAsync(int verificationId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verification = await context.StockVerifications
                .Include(sv => sv.Branch)
                .Include(sv => sv.Counter)
                .Include(sv => sv.Category)
                .Include(sv => sv.VerificationDetails)
                .FirstOrDefaultAsync(sv => sv.Id == verificationId && sv.ClientCode == clientCode);

            if (verification == null)
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Stock verification session not found"
                };
            }

            var response = await MapToStockVerificationDtoAsync(verification, context);
            return new StockVerificationResponseDto
            {
                Success = true,
                Message = "Stock verification retrieved successfully",
                Data = response
            };
        }

        public async Task<StockVerificationReportResponseDto> GetStockVerificationsAsync(StockVerificationReportFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.StockVerifications
                .Include(sv => sv.Branch)
                .Include(sv => sv.Counter)
                .Include(sv => sv.Category)
                .Where(sv => sv.ClientCode == clientCode && sv.IsActive);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(sv => sv.VerificationDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(sv => sv.VerificationDate <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.BranchName))
                query = query.Where(sv => sv.Branch.BranchName == filter.BranchName);

            if (!string.IsNullOrEmpty(filter.CounterName))
                query = query.Where(sv => sv.Counter.CounterName == filter.CounterName);

            if (filter.CategoryId.HasValue)
                query = query.Where(sv => sv.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(sv => sv.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.VerifiedBy))
                query = query.Where(sv => sv.VerifiedBy == filter.VerifiedBy);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / filter.PageSize);

            var verifications = await query
                .OrderByDescending(sv => sv.VerificationDate)
                .ThenByDescending(sv => sv.VerificationTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var data = new List<StockVerificationDto>();
            foreach (var verification in verifications)
            {
                data.Add(await MapToStockVerificationDtoAsync(verification, context));
            }

            var summary = await GetStockVerificationSummaryAsync(clientCode);

            return new StockVerificationReportResponseDto
            {
                Success = true,
                Message = "Stock verifications retrieved successfully",
                TotalRecords = totalRecords,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                Data = data,
                Summary = summary
            };
        }

        public async Task<StockVerificationSummaryDto> GetStockVerificationSummaryAsync(string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verifications = await context.StockVerifications
                .Where(sv => sv.ClientCode == clientCode && sv.IsActive)
                .ToListAsync();

            var summary = new StockVerificationSummaryDto
            {
                TotalSessions = verifications.Count,
                CompletedSessions = verifications.Count(sv => sv.Status == "Completed"),
                InProgressSessions = verifications.Count(sv => sv.Status == "InProgress"),
                TotalItemsVerified = verifications.Sum(sv => sv.TotalItemsScanned),
                TotalMatchedItems = verifications.Sum(sv => sv.MatchedItemsCount),
                TotalUnmatchedItems = verifications.Sum(sv => sv.UnmatchedItemsCount),
                TotalMissingItems = verifications.Sum(sv => sv.MissingItemsCount),
                TotalMatchedValue = verifications.Sum(sv => sv.TotalMatchedValue ?? 0),
                TotalUnmatchedValue = verifications.Sum(sv => sv.TotalUnmatchedValue ?? 0),
                TotalMissingValue = verifications.Sum(sv => sv.TotalMissingValue ?? 0),
                RecentSessions = new List<StockVerificationDto>()
            };

            // Get recent sessions
            var recentVerifications = await context.StockVerifications
                .Include(sv => sv.Branch)
                .Include(sv => sv.Counter)
                .Include(sv => sv.Category)
                .Where(sv => sv.ClientCode == clientCode && sv.IsActive)
                .OrderByDescending(sv => sv.CreatedOn)
                .Take(5)
                .ToListAsync();

            foreach (var verification in recentVerifications)
            {
                summary.RecentSessions.Add(await MapToStockVerificationDtoAsync(verification, context));
            }

            return summary;
        }

        public async Task<List<DateWiseStockVerificationReportDto>> GetDateWiseStockVerificationReportAsync(DateTime startDate, DateTime endDate, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verifications = await context.StockVerifications
                .Include(sv => sv.Branch)
                .Include(sv => sv.Counter)
                .Include(sv => sv.Category)
                .Where(sv => sv.ClientCode == clientCode && 
                            sv.IsActive && 
                            sv.VerificationDate >= startDate.Date && 
                            sv.VerificationDate <= endDate.Date)
                .ToListAsync();

            var dateGroups = verifications
                .GroupBy(sv => sv.VerificationDate)
                .OrderBy(g => g.Key)
                .ToList();

            var report = new List<DateWiseStockVerificationReportDto>();

            foreach (var group in dateGroups)
            {
                var dateReport = new DateWiseStockVerificationReportDto
                {
                    Date = group.Key,
                    TotalSessions = group.Count(),
                    TotalItemsScanned = group.Sum(sv => sv.TotalItemsScanned),
                    TotalMatchedItems = group.Sum(sv => sv.MatchedItemsCount),
                    TotalUnmatchedItems = group.Sum(sv => sv.UnmatchedItemsCount),
                    TotalMissingItems = group.Sum(sv => sv.MissingItemsCount),
                    TotalMatchedValue = group.Sum(sv => sv.TotalMatchedValue ?? 0),
                    TotalUnmatchedValue = group.Sum(sv => sv.TotalUnmatchedValue ?? 0),
                    TotalMissingValue = group.Sum(sv => sv.TotalMissingValue ?? 0),
                    Sessions = new List<StockVerificationDto>()
                };

                foreach (var verification in group)
                {
                    dateReport.Sessions.Add(await MapToStockVerificationDtoAsync(verification, context));
                }

                report.Add(dateReport);
            }

            return report;
        }

        public async Task<StockVerificationResponseDto> CompleteStockVerificationAsync(int verificationId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verification = await context.StockVerifications
                .FirstOrDefaultAsync(sv => sv.Id == verificationId && sv.ClientCode == clientCode);

            if (verification == null)
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Stock verification session not found"
                };
            }

            if (verification.Status != "InProgress")
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Cannot complete a verification session that is not in progress"
                };
            }

            verification.Status = "Completed";
            verification.CompletedOn = DateTime.UtcNow;
            verification.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var response = await MapToStockVerificationDtoAsync(verification, context);
            return new StockVerificationResponseDto
            {
                Success = true,
                Message = "Stock verification session completed successfully",
                Data = response
            };
        }

        public async Task<StockVerificationResponseDto> CancelStockVerificationAsync(int verificationId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var verification = await context.StockVerifications
                .FirstOrDefaultAsync(sv => sv.Id == verificationId && sv.ClientCode == clientCode);

            if (verification == null)
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Stock verification session not found"
                };
            }

            if (verification.Status != "InProgress")
            {
                return new StockVerificationResponseDto
                {
                    Success = false,
                    Message = "Cannot cancel a verification session that is not in progress"
                };
            }

            verification.Status = "Cancelled";
            verification.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var response = await MapToStockVerificationDtoAsync(verification, context);
            return new StockVerificationResponseDto
            {
                Success = true,
                Message = "Stock verification session cancelled successfully",
                Data = response
            };
        }

        public async Task<List<StockVerificationDetailDto>> GetVerificationDetailsByStatusAsync(int verificationId, string status, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var details = await context.StockVerificationDetails
                .Where(svd => svd.StockVerificationId == verificationId && 
                              svd.VerificationStatus == status && 
                              svd.ClientCode == clientCode && 
                              svd.IsActive)
                .ToListAsync();

            var result = new List<StockVerificationDetailDto>();

            foreach (var detail in details)
            {
                var detailDto = new StockVerificationDetailDto
                {
                    Id = detail.Id,
                    ItemCode = detail.ItemCode,
                    RfidCode = detail.RfidCode,
                    VerificationStatus = detail.VerificationStatus,
                    ScannedAt = detail.ScannedAt,
                    ScannedTime = detail.ScannedTime,
                    ScannedBy = detail.ScannedBy,
                    Remarks = detail.Remarks
                };

                // If matched, get product details
                if (status == "Matched")
                {
                    var product = await context.ProductDetails
                        .Include(p => p.Category)
                        .Include(p => p.Product)
                        .Include(p => p.Design)
                        .Include(p => p.Purity)
                        .Include(p => p.Branch)
                        .Include(p => p.Counter)
                        .FirstOrDefaultAsync(p => p.ItemCode == detail.ItemCode && p.ClientCode == clientCode);

                    if (product != null)
                    {
                        detailDto.ProductDetails = new ProductDetailsDto
                        {
                            Id = product.Id,
                            ItemCode = product.ItemCode,
                            CategoryName = product.Category?.CategoryName,
                            ProductName = product.Product?.ProductName,
                            DesignName = product.Design?.DesignName,
                            PurityName = product.Purity?.PurityName,
                            BranchName = product.Branch?.BranchName,
                            CounterName = product.Counter?.CounterName,
                            GrossWeight = product.GrossWeight,
                            NetWeight = product.NetWeight,
                            Mrp = product.Mrp,
                            Status = product.Status
                        };
                    }
                }

                result.Add(detailDto);
            }

            return result;
        }

        #region Private Methods

        private async Task<StockVerificationDto> MapToStockVerificationDtoAsync(StockVerification verification, ClientDbContext context)
        {
            var dto = new StockVerificationDto
            {
                Id = verification.Id,
                VerificationSessionName = verification.VerificationSessionName,
                Description = verification.Description,
                VerificationDate = verification.VerificationDate,
                VerificationTime = verification.VerificationTime,
                BranchId = verification.BranchId,
                BranchName = verification.Branch?.BranchName ?? "Unknown",
                CounterId = verification.CounterId,
                CounterName = verification.Counter?.CounterName ?? "Unknown",
                CategoryId = verification.CategoryId,
                CategoryName = verification.Category?.CategoryName ?? "Unknown",
                TotalItemsScanned = verification.TotalItemsScanned,
                MatchedItemsCount = verification.MatchedItemsCount,
                UnmatchedItemsCount = verification.UnmatchedItemsCount,
                MissingItemsCount = verification.MissingItemsCount,
                TotalMatchedValue = verification.TotalMatchedValue,
                TotalUnmatchedValue = verification.TotalUnmatchedValue,
                TotalMissingValue = verification.TotalMissingValue,
                VerifiedBy = verification.VerifiedBy,
                Status = verification.Status,
                Remarks = verification.Remarks,
                CreatedOn = verification.CreatedOn,
                CompletedOn = verification.CompletedOn,
                VerificationDetails = new List<StockVerificationDetailDto>()
            };

            // Map verification details
            if (verification.VerificationDetails != null)
            {
                foreach (var detail in verification.VerificationDetails.Where(d => d.IsActive))
                {
                    var detailDto = new StockVerificationDetailDto
                    {
                        Id = detail.Id,
                        ItemCode = detail.ItemCode,
                        RfidCode = detail.RfidCode,
                        VerificationStatus = detail.VerificationStatus,
                        ScannedAt = detail.ScannedAt,
                        ScannedTime = detail.ScannedTime,
                        ScannedBy = detail.ScannedBy,
                        Remarks = detail.Remarks
                    };

                    // If matched, get product details
                    if (detail.VerificationStatus == "Matched")
                    {
                        var product = await context.ProductDetails
                            .Include(p => p.Category)
                            .Include(p => p.Product)
                            .Include(p => p.Design)
                            .Include(p => p.Purity)
                            .Include(p => p.Branch)
                            .Include(p => p.Counter)
                            .FirstOrDefaultAsync(p => p.ItemCode == detail.ItemCode);

                        if (product != null)
                        {
                            detailDto.ProductDetails = new ProductDetailsDto
                            {
                                Id = product.Id,
                                ItemCode = product.ItemCode,
                                CategoryName = product.Category?.CategoryName,
                                ProductName = product.Product?.ProductName,
                                DesignName = product.Design?.DesignName,
                                PurityName = product.Purity?.PurityName,
                                BranchName = product.Branch?.BranchName,
                                CounterName = product.Counter?.CounterName,
                                GrossWeight = product.GrossWeight,
                                NetWeight = product.NetWeight,
                                Mrp = product.Mrp,
                                Status = product.Status
                            };
                        }
                    }

                    dto.VerificationDetails.Add(detailDto);
                }
            }

            return dto;
        }

        private async Task CalculateVerificationValuesAsync(int verificationId, ClientDbContext context)
        {
            var verification = await context.StockVerifications
                .FirstOrDefaultAsync(sv => sv.Id == verificationId);

            if (verification == null) return;

            var matchedDetails = await context.StockVerificationDetails
                .Where(svd => svd.StockVerificationId == verificationId && 
                              svd.VerificationStatus == "Matched" && 
                              svd.IsActive)
                .ToListAsync();

            var unmatchedDetails = await context.StockVerificationDetails
                .Where(svd => svd.StockVerificationId == verificationId && 
                              svd.VerificationStatus == "Unmatched" && 
                              svd.IsActive)
                .ToListAsync();

            // Calculate matched values
            decimal totalMatchedValue = 0;
            foreach (var detail in matchedDetails)
            {
                var product = await context.ProductDetails
                    .Where(p => p.ItemCode == detail.ItemCode)
                    .Select(p => p.Mrp)
                    .FirstOrDefaultAsync();

                if (product.HasValue)
                    totalMatchedValue += product.Value;
            }

            // Calculate unmatched values (items that exist in system but weren't scanned)
            var expectedItems = await context.ProductDetails
                .Where(p => p.CategoryId == verification.CategoryId && 
                           p.BranchId == verification.BranchId && 
                           p.CounterId == verification.CounterId &&
                           p.ClientCode == verification.ClientCode)
                .ToListAsync();

            var scannedItemCodes = matchedDetails.Select(d => d.ItemCode).ToList();
            var missingItems = expectedItems.Where(p => !scannedItemCodes.Contains(p.ItemCode)).ToList();

            verification.MissingItemsCount = missingItems.Count;
            verification.TotalMatchedValue = totalMatchedValue;
            verification.TotalMissingValue = missingItems.Sum(p => p.Mrp ?? 0);

            await context.SaveChangesAsync();
        }

        #endregion
    }
}
