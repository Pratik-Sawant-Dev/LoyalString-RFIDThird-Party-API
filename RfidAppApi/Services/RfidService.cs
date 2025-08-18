using RfidAppApi.DTOs;
using RfidAppApi.Models;
using RfidAppApi.Repositories;
using RfidAppApi.Data;
using Microsoft.EntityFrameworkCore;

namespace RfidAppApi.Services
{
    public class RfidService : IRfidService
    {
        private readonly IClientService _clientService;

        public RfidService(IClientService clientService)
        {
            _clientService = clientService;
        }

        public Task<IEnumerable<RfidDto>> GetAllRfidsAsync()
        {
            // This method is not used in the multi-tenant architecture
            // All operations should be client-specific
            throw new NotImplementedException("GetAllRfidsAsync is not supported in multi-tenant architecture. Use GetRfidsByClientAsync instead.");
        }

        public async Task<IEnumerable<RfidDto>> GetRfidsByClientAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            var rfids = await clientContext.Rfids.Where(r => r.IsActive).ToListAsync();
            return rfids.Select(MapToDto);
        }

        public async Task<RfidDto?> GetRfidByCodeAsync(string rfidCode, string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            var rfid = await clientContext.Rfids.FirstOrDefaultAsync(r => r.RFIDCode == rfidCode && r.IsActive);
            return rfid != null ? MapToDto(rfid) : null;
        }

        public async Task<RfidDto> CreateRfidAsync(CreateRfidDto createRfidDto)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(createRfidDto.ClientCode);
            
            // Check if RFID code already exists
            if (await clientContext.Rfids.AnyAsync(r => r.RFIDCode == createRfidDto.RFIDCode))
            {
                throw new InvalidOperationException($"RFID code {createRfidDto.RFIDCode} already exists for client {createRfidDto.ClientCode}.");
            }

            var rfid = new Rfid
            {
                RFIDCode = createRfidDto.RFIDCode,
                EPCValue = createRfidDto.EPCValue,
                ClientCode = createRfidDto.ClientCode,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            clientContext.Rfids.Add(rfid);
            await clientContext.SaveChangesAsync();
            
            return MapToDto(rfid);
        }

        public async Task<RfidDto> UpdateRfidAsync(string rfidCode, string clientCode, UpdateRfidDto updateRfidDto)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            var rfid = await clientContext.Rfids.FirstOrDefaultAsync(r => r.RFIDCode == rfidCode);
            if (rfid == null)
            {
                throw new InvalidOperationException($"RFID code {rfidCode} not found for client {clientCode}.");
            }

            if (updateRfidDto.EPCValue != null)
            {
                rfid.EPCValue = updateRfidDto.EPCValue;
            }

            if (updateRfidDto.IsActive.HasValue)
            {
                rfid.IsActive = updateRfidDto.IsActive.Value;
            }

            await clientContext.SaveChangesAsync();
            return MapToDto(rfid);
        }

        public async Task DeleteRfidAsync(string rfidCode, string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            var rfid = await clientContext.Rfids.FirstOrDefaultAsync(r => r.RFIDCode == rfidCode);
            if (rfid == null)
            {
                throw new InvalidOperationException($"RFID code {rfidCode} not found for client {clientCode}.");
            }

            // Check if RFID is assigned to any product
            var isAssigned = await clientContext.ProductRfidAssignments
                .AnyAsync(pr => pr.RFIDCode == rfidCode && pr.IsActive);
            
            if (isAssigned)
            {
                throw new InvalidOperationException($"Cannot delete RFID {rfidCode} as it is currently assigned to a product.");
            }

            clientContext.Rfids.Remove(rfid);
            await clientContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<RfidDto>> GetAvailableRfidsAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            // Get all active RFID tags that are not assigned to any product
            var assignedRfidCodes = await clientContext.ProductRfidAssignments
                .Where(pr => pr.IsActive)
                .Select(pr => pr.RFIDCode)
                .ToListAsync();

            var availableRfids = await clientContext.Rfids
                .Where(r => r.IsActive && !assignedRfidCodes.Contains(r.RFIDCode))
                .ToListAsync();

            return availableRfids.Select(MapToDto);
        }

        public async Task<IEnumerable<RfidDto>> GetActiveRfidsAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            var rfids = await clientContext.Rfids
                .Where(r => r.IsActive)
                .ToListAsync();

            return rfids.Select(MapToDto);
        }

        public async Task<int> GetRfidCountByClientAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            return await clientContext.Rfids
                .Where(r => r.IsActive)
                .CountAsync();
        }

        public async Task<UsedRfidAnalysisDto> GetUsedRfidAnalysisAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            // Get all RFID tags that are currently assigned to products
            var usedRfids = await clientContext.ProductRfidAssignments
                .Where(pr => pr.IsActive)
                .Join(
                    clientContext.Rfids,
                    pr => pr.RFIDCode,
                    r => r.RFIDCode,
                    (pr, r) => new UsedRfidDetailDto
                    {
                        RFIDCode = r.RFIDCode,
                        EPCValue = r.EPCValue,
                        ProductId = pr.ProductId,
                        AssignedOn = pr.AssignedOn,
                        ProductInfo = $"Product ID: {pr.ProductId}"
                    }
                )
                .ToListAsync();

            var totalUsedCount = usedRfids.Count;
            var summary = $"Found {totalUsedCount} used RFID tags out of total RFID inventory for client {clientCode}";

            return new UsedRfidAnalysisDto
            {
                TotalUsedCount = totalUsedCount,
                UsedRfids = usedRfids,
                Summary = summary
            };
        }

        public async Task<UnusedRfidAnalysisDto> GetUnusedRfidAnalysisAsync(string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            // Get all RFID tags that are not currently assigned to products
            var assignedRfidCodes = await clientContext.ProductRfidAssignments
                .Where(pr => pr.IsActive)
                .Select(pr => pr.RFIDCode)
                .ToListAsync();

            var unusedRfids = await clientContext.Rfids
                .Where(r => r.IsActive && !assignedRfidCodes.Contains(r.RFIDCode))
                .ToListAsync();

            var totalUnusedCount = unusedRfids.Count;
            var summary = $"Found {totalUnusedCount} unused RFID tags out of total RFID inventory for client {clientCode}";

            return new UnusedRfidAnalysisDto
            {
                TotalUnusedCount = totalUnusedCount,
                UnusedRfids = unusedRfids.Select(r => new UnusedRfidDetailDto
                {
                    RFIDCode = r.RFIDCode,
                    EPCValue = r.EPCValue,
                    CreatedOn = r.CreatedOn,
                    IsActive = r.IsActive
                }).ToList(),
                Summary = summary
            };
        }

        public async Task<RfidScanResponseDto> ScanProductsByEpcValueAsync(RfidScanRequestDto request, string clientCode)
        {
            using var clientContext = await _clientService.GetClientDbContextAsync(clientCode);
            
            var response = new RfidScanResponseDto();
            var allEpcValues = new List<string>();
            
            // Collect all EPC values to scan
            if (!string.IsNullOrWhiteSpace(request.EpcValue))
            {
                allEpcValues.Add(request.EpcValue.Trim());
            }
            
            if (request.EpcValues != null && request.EpcValues.Any())
            {
                allEpcValues.AddRange(request.EpcValues.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
            }
            
            // Remove duplicates
            allEpcValues = allEpcValues.Distinct().ToList();
            
            if (!allEpcValues.Any())
            {
                response.Success = false;
                response.Message = "No EPC values provided for scanning";
                return response;
            }
            
            Console.WriteLine($"Scanning {allEpcValues.Count} EPC values: {string.Join(", ", allEpcValues)}");
            
            var scanResults = new List<EpcScanResultDto>();
            var unmatchedEpcValues = new List<string>();
            var totalProductsFound = 0;
            
            foreach (var epcValue in allEpcValues)
            {
                Console.WriteLine($"Processing EPC value: {epcValue}");
                
                // Find RFID tag with this EPC value
                var rfid = await clientContext.Rfids
                    .FirstOrDefaultAsync(r => r.EPCValue == epcValue && r.IsActive);
                
                if (rfid == null)
                {
                    Console.WriteLine($"No RFID found for EPC value: {epcValue}");
                    unmatchedEpcValues.Add(epcValue);
                    continue;
                }
                
                Console.WriteLine($"Found RFID {rfid.RFIDCode} for EPC value: {epcValue}");
                
                // Find all products assigned to this RFID code
                var productAssignments = await clientContext.ProductRfidAssignments
                    .Where(pr => pr.RFIDCode == rfid.RFIDCode && pr.IsActive)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Category)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Product)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Design)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Purity)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Branch)
                    .Include(pr => pr.Product)
                        .ThenInclude(p => p.Counter)
                    .ToListAsync();
                
                if (!productAssignments.Any())
                {
                    Console.WriteLine($"No products found for RFID {rfid.RFIDCode}");
                    unmatchedEpcValues.Add(epcValue);
                    continue;
                }
                
                Console.WriteLine($"Found {productAssignments.Count} products for RFID {rfid.RFIDCode}");
                
                // Map the products to the response DTO
                var scannedProducts = productAssignments.Select(pa => new ScannedProductDto
                {
                    ProductId = pa.Product.Id,
                    ItemCode = pa.Product.ItemCode,
                    RFIDCode = pa.RFIDCode,
                    EPCValue = rfid.EPCValue,
                    AssignedOn = pa.AssignedOn,
                    Status = pa.Product.Status,
                    CategoryName = pa.Product.Category?.CategoryName ?? "Unknown",
                    ProductName = pa.Product.Product?.ProductName ?? "Unknown",
                    DesignName = pa.Product.Design?.DesignName ?? "Unknown",
                    PurityName = pa.Product.Purity?.PurityName ?? "Unknown",
                    BranchName = pa.Product.Branch?.BranchName ?? "Unknown",
                    CounterName = pa.Product.Counter?.CounterName ?? "Unknown",
                    GrossWeight = pa.Product.GrossWeight,
                    NetWeight = pa.Product.NetWeight,
                    StoneWeight = pa.Product.StoneWeight,
                    DiamondHeight = pa.Product.DiamondHeight,
                    BoxDetails = pa.Product.BoxDetails,
                    Size = pa.Product.Size,
                    StoneAmount = pa.Product.StoneAmount,
                    DiamondAmount = pa.Product.DiamondAmount,
                    HallmarkAmount = pa.Product.HallmarkAmount,
                    MakingPerGram = pa.Product.MakingPerGram,
                    MakingPercentage = pa.Product.MakingPercentage,
                    MakingFixedAmount = pa.Product.MakingFixedAmount,
                    Mrp = pa.Product.Mrp,
                    ImageUrl = pa.Product.ImageUrl
                }).ToList();
                
                var epcResult = new EpcScanResultDto
                {
                    EpcValue = epcValue,
                    RfidCode = rfid.RFIDCode,
                    ProductCount = scannedProducts.Count,
                    Products = scannedProducts
                };
                
                scanResults.Add(epcResult);
                totalProductsFound += scannedProducts.Count;
                
                Console.WriteLine($"Added {scannedProducts.Count} products for EPC {epcValue}");
            }
            
            // Build final response
            response.Success = true;
            response.TotalProductsFound = totalProductsFound;
            response.ScanResults = scanResults;
            response.UnmatchedEpcValues = unmatchedEpcValues;
            
            if (totalProductsFound > 0)
            {
                response.Message = $"Scan completed successfully. Found {totalProductsFound} products across {scanResults.Count} EPC values.";
                if (unmatchedEpcValues.Any())
                {
                    response.Message += $" {unmatchedEpcValues.Count} EPC values had no associated products.";
                }
            }
            else
            {
                response.Message = "Scan completed but no products found for any of the provided EPC values.";
            }
            
            Console.WriteLine($"Scan completed. Total products found: {totalProductsFound}, Unmatched EPCs: {unmatchedEpcValues.Count}");
            
            return response;
        }

        private static RfidDto MapToDto(Rfid rfid)
        {
            return new RfidDto
            {
                RFIDCode = rfid.RFIDCode,
                EPCValue = rfid.EPCValue,
                ClientCode = rfid.ClientCode,
                IsActive = rfid.IsActive,
                CreatedOn = rfid.CreatedOn
            };
        }
    }
} 