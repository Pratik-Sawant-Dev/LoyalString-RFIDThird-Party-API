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
            
            // Get all active RFID tags that are not assigned to any product
            var assignedRfidCodes = await clientContext.ProductRfidAssignments
                .Where(pr => pr.IsActive)
                .Select(pr => pr.RFIDCode)
                .ToListAsync();

            var unusedRfids = await clientContext.Rfids
                .Where(r => r.IsActive && !assignedRfidCodes.Contains(r.RFIDCode))
                .Select(r => new UnusedRfidDetailDto
                {
                    RFIDCode = r.RFIDCode,
                    EPCValue = r.EPCValue,
                    CreatedOn = r.CreatedOn,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            var totalUnusedCount = unusedRfids.Count;
            var summary = $"Found {totalUnusedCount} unused RFID tags out of total RFID inventory for client {clientCode}";

            return new UnusedRfidAnalysisDto
            {
                TotalUnusedCount = totalUnusedCount,
                UnusedRfids = unusedRfids,
                Summary = summary
            };
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