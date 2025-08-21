using Microsoft.EntityFrameworkCore;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Security.Claims;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing stock transfers between branches, counters, and boxes
    /// </summary>
    public class StockTransferService : IStockTransferService
    {
        private readonly IClientService _clientService;

        public StockTransferService(IClientService clientService)
        {
            _clientService = clientService;
        }

        public async Task<StockTransferResponseDto> CreateTransferAsync(CreateStockTransferDto createDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Validate the transfer request
            if (!await ValidateTransferAsync(createDto, clientCode))
            {
                throw new InvalidOperationException("Transfer validation failed. Please check the transfer details.");
            }

            // Generate unique transfer number
            var transferNumber = await GenerateTransferNumberAsync(context, clientCode);

            // Create the transfer record
            var transfer = new StockTransfer
            {
                ClientCode = clientCode,
                TransferNumber = transferNumber,
                ProductId = createDto.ProductId,
                RfidCode = createDto.RfidCode,
                TransferType = DetermineTransferType(createDto),
                SourceBranchId = createDto.SourceBranchId,
                SourceCounterId = createDto.SourceCounterId,
                SourceBoxId = createDto.SourceBoxId,
                DestinationBranchId = createDto.DestinationBranchId,
                DestinationCounterId = createDto.DestinationCounterId,
                DestinationBoxId = createDto.DestinationBoxId,
                Status = TransferStatuses.Pending,
                TransferDate = DateTime.UtcNow,
                Reason = createDto.Reason,
                Remarks = createDto.Remarks,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            context.StockTransfers.Add(transfer);
            await context.SaveChangesAsync();

            // Create stock movement record
            await CreateStockMovementRecordAsync(context, transfer, "Transfer", clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<BulkTransferResponseDto> CreateBulkTransfersAsync(BulkStockTransferDto bulkDto, string clientCode)
        {
            var response = new BulkTransferResponseDto
            {
                TotalTransfers = bulkDto.Transfers.Count,
                SuccessfullyCreated = 0,
                Failed = 0,
                CreatedTransfers = new List<StockTransferResponseDto>(),
                Errors = new List<string>()
            };

            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            foreach (var transferDto in bulkDto.Transfers)
            {
                try
                {
                    // Apply common reason and remarks if not specified
                    if (string.IsNullOrEmpty(transferDto.Reason) && !string.IsNullOrEmpty(bulkDto.CommonReason))
                        transferDto.Reason = bulkDto.CommonReason;

                    if (string.IsNullOrEmpty(transferDto.Remarks) && !string.IsNullOrEmpty(bulkDto.CommonRemarks))
                        transferDto.Remarks = bulkDto.CommonRemarks;

                    var transfer = await CreateTransferAsync(transferDto, clientCode);
                    response.CreatedTransfers.Add(transfer);
                    response.SuccessfullyCreated++;
                }
                catch (Exception ex)
                {
                    response.Failed++;
                    response.Errors.Add($"Transfer for Product ID {transferDto.ProductId} failed: {ex.Message}");
                }
            }

            return response;
        }

        public async Task<StockTransferResponseDto?> GetTransferAsync(int transferId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                return null;

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<List<StockTransferResponseDto>> GetTransfersAsync(TransferFilterDto filter, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.StockTransfers
                .Where(t => t.ClientCode == clientCode && t.IsActive);

            // Apply filters
            if (filter.ProductId.HasValue)
                query = query.Where(t => t.ProductId == filter.ProductId.Value);

            if (!string.IsNullOrEmpty(filter.RfidCode))
                query = query.Where(t => t.RfidCode == filter.RfidCode);

            if (!string.IsNullOrEmpty(filter.TransferType))
                query = query.Where(t => t.TransferType == filter.TransferType);

            if (filter.SourceBranchId.HasValue)
                query = query.Where(t => t.SourceBranchId == filter.SourceBranchId.Value);

            if (filter.SourceCounterId.HasValue)
                query = query.Where(t => t.SourceCounterId == filter.SourceCounterId.Value);

            if (filter.DestinationBranchId.HasValue)
                query = query.Where(t => t.DestinationBranchId == filter.DestinationBranchId.Value);

            if (filter.DestinationCounterId.HasValue)
                query = query.Where(t => t.DestinationCounterId == filter.DestinationCounterId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.TransferDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.TransferDate <= filter.ToDate.Value);

            // Apply pagination
            var totalCount = await query.CountAsync();
            var transfers = await query
                .OrderByDescending(t => t.TransferDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new List<StockTransferResponseDto>();
            foreach (var transfer in transfers)
            {
                result.Add(await MapToTransferResponseAsync(context, transfer));
            }

            return result;
        }

        public async Task<StockTransferResponseDto> UpdateTransferStatusAsync(int transferId, UpdateTransferStatusDto updateDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                throw new InvalidOperationException($"Transfer with ID {transferId} not found");

            if (transfer.Status == TransferStatuses.Completed)
                throw new InvalidOperationException("Cannot update status of a completed transfer");

            // Update status
            transfer.Status = updateDto.Status;
            transfer.Remarks = updateDto.Remarks ?? transfer.Remarks;
            transfer.UpdatedOn = DateTime.UtcNow;

            // Handle specific status updates
            switch (updateDto.Status)
            {
                case TransferStatuses.InTransit:
                    // No additional logic needed
                    break;
                case TransferStatuses.Cancelled:
                    transfer.UpdatedOn = DateTime.UtcNow;
                    break;
                case TransferStatuses.Rejected:
                    if (string.IsNullOrEmpty(updateDto.RejectionReason))
                        throw new ArgumentException("Rejection reason is required when rejecting a transfer");
                    transfer.RejectionReason = updateDto.RejectionReason;
                    break;
            }

            await context.SaveChangesAsync();

            // Update stock movement record
            await UpdateStockMovementRecordAsync(context, transfer, updateDto.Status, clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<StockTransferResponseDto> ApproveTransferAsync(int transferId, ApproveTransferDto approveDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                throw new InvalidOperationException($"Transfer with ID {transferId} not found");

            if (transfer.Status != TransferStatuses.Pending)
                throw new InvalidOperationException($"Cannot approve transfer with status {transfer.Status}");

            transfer.Status = TransferStatuses.InTransit;
            transfer.ApprovedBy = approveDto.ApprovedBy;
            transfer.ApprovedOn = DateTime.UtcNow;
            transfer.Remarks = approveDto.Remarks ?? transfer.Remarks;
            transfer.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Update stock movement record
            await UpdateStockMovementRecordAsync(context, transfer, TransferStatuses.InTransit, clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<StockTransferResponseDto> RejectTransferAsync(int transferId, RejectTransferDto rejectDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                throw new InvalidOperationException($"Transfer with ID {transferId} not found");

            if (transfer.Status != TransferStatuses.Pending)
                throw new InvalidOperationException($"Cannot reject transfer with status {transfer.Status}");

            transfer.Status = TransferStatuses.Rejected;
            transfer.RejectedBy = rejectDto.RejectedBy;
            transfer.RejectedOn = DateTime.UtcNow;
            transfer.RejectionReason = rejectDto.RejectionReason;
            transfer.Remarks = rejectDto.Remarks ?? transfer.Remarks;
            transfer.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Update stock movement record
            await UpdateStockMovementRecordAsync(context, transfer, TransferStatuses.Rejected, clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<StockTransferResponseDto> CompleteTransferAsync(int transferId, string completedBy, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                throw new InvalidOperationException($"Transfer with ID {transferId} not found");

            if (transfer.Status != TransferStatuses.InTransit)
                throw new InvalidOperationException($"Cannot complete transfer with status {transfer.Status}");

            // Update transfer status
            transfer.Status = TransferStatuses.Completed;
            transfer.CompletedDate = DateTime.UtcNow;
            transfer.UpdatedOn = DateTime.UtcNow;

            // Move the product to destination location
            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == transfer.ProductId && p.ClientCode == clientCode);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {transfer.ProductId} not found");

            // Update product location
            product.BranchId = transfer.DestinationBranchId;
            product.CounterId = transfer.DestinationCounterId;
            product.BoxId = transfer.DestinationBoxId;

            await context.SaveChangesAsync();

            // Update stock movement record
            await UpdateStockMovementRecordAsync(context, transfer, TransferStatuses.Completed, clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<StockTransferResponseDto> CancelTransferAsync(int transferId, string cancelledBy, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfer = await context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && t.ClientCode == clientCode);

            if (transfer == null)
                throw new InvalidOperationException($"Transfer with ID {transferId} not found");

            if (transfer.Status == TransferStatuses.Completed)
                throw new InvalidOperationException("Cannot cancel a completed transfer");

            transfer.Status = TransferStatuses.Cancelled;
            transfer.UpdatedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Update stock movement record
            await UpdateStockMovementRecordAsync(context, transfer, TransferStatuses.Cancelled, clientCode);

            return await MapToTransferResponseAsync(context, transfer);
        }

        public async Task<TransferSummaryDto> GetTransferSummaryAsync(string clientCode, DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.StockTransfers.Where(t => t.ClientCode == clientCode && t.IsActive);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransferDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransferDate <= toDate.Value);

            var transfers = await query.ToListAsync();

            var summary = new TransferSummaryDto
            {
                TotalTransfers = transfers.Count,
                PendingTransfers = transfers.Count(t => t.Status == TransferStatuses.Pending),
                InTransitTransfers = transfers.Count(t => t.Status == TransferStatuses.InTransit),
                CompletedTransfers = transfers.Count(t => t.Status == TransferStatuses.Completed),
                CancelledTransfers = transfers.Count(t => t.Status == TransferStatuses.Cancelled),
                RejectedTransfers = transfers.Count(t => t.Status == TransferStatuses.Rejected),
                TotalValue = 0, // Will be calculated below
                TransferTypeSummary = new List<TransferTypeSummary>(),
                BranchTransferSummary = new List<BranchTransferSummary>()
            };

            // Calculate total value and transfer type summary
            var transferTypeGroups = transfers.GroupBy(t => t.TransferType);
            foreach (var group in transferTypeGroups)
            {
                var typeSummary = new TransferTypeSummary
                {
                    TransferType = group.Key,
                    Count = group.Count(),
                    TotalValue = 0 // Will be calculated if needed
                };
                summary.TransferTypeSummary.Add(typeSummary);
            }

            // Calculate branch transfer summary
            var branchGroups = transfers.GroupBy(t => t.SourceBranchId);
            foreach (var group in branchGroups)
            {
                var branch = await context.BranchMasters.FirstOrDefaultAsync(b => b.BranchId == group.Key);
                var branchSummary = new BranchTransferSummary
                {
                    BranchId = group.Key,
                    BranchName = branch?.BranchName ?? "Unknown",
                    IncomingTransfers = group.Count(t => t.DestinationBranchId == group.Key),
                    OutgoingTransfers = group.Count(t => t.SourceBranchId == group.Key),
                    IncomingValue = 0, // Will be calculated if needed
                    OutgoingValue = 0  // Will be calculated if needed
                };
                summary.BranchTransferSummary.Add(branchSummary);
            }

            return summary;
        }

        public async Task<List<StockTransferResponseDto>> GetTransfersByProductAsync(int productId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfers = await context.StockTransfers
                .Where(t => t.ProductId == productId && t.ClientCode == clientCode && t.IsActive)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();

            var result = new List<StockTransferResponseDto>();
            foreach (var transfer in transfers)
            {
                result.Add(await MapToTransferResponseAsync(context, transfer));
            }

            return result;
        }

        public async Task<List<StockTransferResponseDto>> GetTransfersByRfidAsync(string rfidCode, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var transfers = await context.StockTransfers
                .Where(t => t.RfidCode == rfidCode && t.ClientCode == clientCode && t.IsActive)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();

            var result = new List<StockTransferResponseDto>();
            foreach (var transfer in transfers)
            {
                result.Add(await MapToTransferResponseAsync(context, transfer));
            }

            return result;
        }

        public async Task<List<StockTransferResponseDto>> GetPendingTransfersByLocationAsync(int branchId, int counterId, int? boxId, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            var query = context.StockTransfers
                .Where(t => t.ClientCode == clientCode && 
                           t.Status == TransferStatuses.Pending &&
                           t.SourceBranchId == branchId &&
                           t.SourceCounterId == counterId);

            if (boxId.HasValue)
                query = query.Where(t => t.SourceBoxId == boxId);

            var transfers = await query
                .OrderBy(t => t.TransferDate)
                .ToListAsync();

            var result = new List<StockTransferResponseDto>();
            foreach (var transfer in transfers)
            {
                result.Add(await MapToTransferResponseAsync(context, transfer));
            }

            return result;
        }

        public async Task<bool> ValidateTransferAsync(CreateStockTransferDto createDto, string clientCode)
        {
            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            // Check if product exists and is active
            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == createDto.ProductId && p.ClientCode == clientCode);

            if (product == null)
                return false;

            if (product.Status?.ToLower() != "active")
                return false;

            // Check if source location matches current product location
            if (product.BranchId != createDto.SourceBranchId || 
                product.CounterId != createDto.SourceCounterId ||
                product.BoxId != createDto.SourceBoxId)
                return false;

            // Check if destination location is valid
            var destinationBranch = await context.BranchMasters
                .FirstOrDefaultAsync(b => b.BranchId == createDto.DestinationBranchId && b.ClientCode == clientCode);

            if (destinationBranch == null)
                return false;

            var destinationCounter = await context.CounterMasters
                .FirstOrDefaultAsync(c => c.CounterId == createDto.DestinationCounterId && c.BranchId == createDto.DestinationBranchId);

            if (destinationCounter == null)
                return false;

            // Check if there are no pending transfers for this product
            var pendingTransfer = await context.StockTransfers
                .AnyAsync(t => t.ProductId == createDto.ProductId && 
                              t.Status == TransferStatuses.Pending && 
                              t.ClientCode == clientCode);

            if (pendingTransfer)
                return false;

            return true;
        }

        public async Task<List<StockTransferResponseDto>> GetProductTransferHistoryAsync(int productId, string clientCode)
        {
            return await GetTransfersByProductAsync(productId, clientCode);
        }

        #region Private Helper Methods

        private async Task<string> GenerateTransferNumberAsync(ClientDbContext context, string clientCode)
        {
            var today = DateTime.UtcNow.Date;
            var count = await context.StockTransfers
                .CountAsync(t => t.ClientCode == clientCode && 
                                t.CreatedOn.Date == today);

            return $"TRF-{clientCode}-{today:yyyyMMdd}-{count + 1:D4}";
        }

        private string DetermineTransferType(CreateStockTransferDto createDto)
        {
            if (createDto.SourceBranchId != createDto.DestinationBranchId)
                return TransferTypes.Branch;
            else if (createDto.SourceCounterId != createDto.DestinationCounterId)
                return TransferTypes.Counter;
            else if (createDto.SourceBoxId != createDto.DestinationBoxId)
                return TransferTypes.Box;
            else
                return TransferTypes.Mixed;
        }

        private async Task CreateStockMovementRecordAsync(ClientDbContext context, StockTransfer transfer, string movementType, string clientCode)
        {
            var product = await context.ProductDetails
                .FirstOrDefaultAsync(p => p.Id == transfer.ProductId);

            if (product == null) return;

            var stockMovement = new StockMovement
            {
                ClientCode = clientCode,
                ProductId = transfer.ProductId,
                RfidCode = transfer.RfidCode,
                MovementType = movementType,
                Quantity = 1,
                UnitPrice = product.Mrp,
                TotalAmount = product.Mrp,
                BranchId = transfer.SourceBranchId,
                CounterId = transfer.SourceCounterId,
                CategoryId = product.CategoryId,
                ReferenceNumber = transfer.TransferNumber,
                ReferenceType = "Transfer",
                Remarks = $"Stock transfer initiated: {transfer.TransferNumber}",
                MovementDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            context.StockMovements.Add(stockMovement);
            await context.SaveChangesAsync();
        }

        private async Task UpdateStockMovementRecordAsync(ClientDbContext context, StockTransfer transfer, string newStatus, string clientCode)
        {
            var existingMovement = await context.StockMovements
                .FirstOrDefaultAsync(sm => sm.ReferenceNumber == transfer.TransferNumber && 
                                         sm.ReferenceType == "Transfer" &&
                                         sm.ClientCode == clientCode);

            if (existingMovement != null)
            {
                existingMovement.MovementType = newStatus;
                existingMovement.Remarks = $"Transfer status updated to: {newStatus}";
                existingMovement.UpdatedOn = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        private async Task<StockTransferResponseDto> MapToTransferResponseAsync(ClientDbContext context, StockTransfer transfer)
        {
            // Get product details
            var product = await context.ProductDetails
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Include(p => p.Design)
                .Include(p => p.Purity)
                .FirstOrDefaultAsync(p => p.Id == transfer.ProductId);

            // Get location details
            var sourceBranch = await context.BranchMasters.FirstOrDefaultAsync(b => b.BranchId == transfer.SourceBranchId);
            var sourceCounter = await context.CounterMasters.FirstOrDefaultAsync(c => c.CounterId == transfer.SourceCounterId);
            var sourceBox = transfer.SourceBoxId.HasValue ? await context.BoxMasters.FirstOrDefaultAsync(b => b.BoxId == transfer.SourceBoxId) : null;

            var destinationBranch = await context.BranchMasters.FirstOrDefaultAsync(b => b.BranchId == transfer.DestinationBranchId);
            var destinationCounter = await context.CounterMasters.FirstOrDefaultAsync(c => c.CounterId == transfer.DestinationCounterId);
            var destinationBox = transfer.DestinationBoxId.HasValue ? await context.BoxMasters.FirstOrDefaultAsync(b => b.BoxId == transfer.DestinationBoxId) : null;

            return new StockTransferResponseDto
            {
                Id = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                ProductId = transfer.ProductId,
                RfidCode = transfer.RfidCode,
                TransferType = transfer.TransferType,
                Status = transfer.Status,
                TransferDate = transfer.TransferDate,
                CompletedDate = transfer.CompletedDate,
                Reason = transfer.Reason,
                Remarks = transfer.Remarks,
                ApprovedBy = transfer.ApprovedBy,
                ApprovedOn = transfer.ApprovedOn,
                RejectedBy = transfer.RejectedBy,
                RejectedOn = transfer.RejectedOn,
                RejectionReason = transfer.RejectionReason,
                CreatedOn = transfer.CreatedOn,
                UpdatedOn = transfer.UpdatedOn,
                SourceLocation = new LocationInfo
                {
                    BranchId = transfer.SourceBranchId,
                    BranchName = sourceBranch?.BranchName ?? "Unknown",
                    CounterId = transfer.SourceCounterId,
                    CounterName = sourceCounter?.CounterName ?? "Unknown",
                    BoxId = transfer.SourceBoxId,
                    BoxName = sourceBox?.BoxName
                },
                DestinationLocation = new LocationInfo
                {
                    BranchId = transfer.DestinationBranchId,
                    BranchName = destinationBranch?.BranchName ?? "Unknown",
                    CounterId = transfer.DestinationCounterId,
                    CounterName = destinationCounter?.CounterName ?? "Unknown",
                    BoxId = transfer.DestinationBoxId,
                    BoxName = destinationBox?.BoxName
                },
                Product = new ProductInfo
                {
                    ItemCode = product?.ItemCode ?? "Unknown",
                    CategoryName = product?.Category?.CategoryName ?? "Unknown",
                    ProductName = product?.Product?.ProductName ?? "Unknown",
                    DesignName = product?.Design?.DesignName ?? "Unknown",
                    PurityName = product?.Purity?.PurityName ?? "Unknown",
                    GrossWeight = product?.GrossWeight,
                    NetWeight = product?.NetWeight,
                    Mrp = product?.Mrp,
                    ImageUrl = product?.ImageUrl
                }
            };
        }

        #endregion
    }
}
