using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IRfidService
    {
        Task<IEnumerable<RfidDto>> GetAllRfidsAsync();
        Task<IEnumerable<RfidDto>> GetRfidsByClientAsync(string clientCode);
        Task<RfidDto?> GetRfidByCodeAsync(string rfidCode, string clientCode);
        Task<RfidDto> CreateRfidAsync(CreateRfidDto createRfidDto);
        Task<RfidDto> UpdateRfidAsync(string rfidCode, string clientCode, UpdateRfidDto updateRfidDto);
        Task DeleteRfidAsync(string rfidCode, string clientCode);
        Task<IEnumerable<RfidDto>> GetAvailableRfidsAsync(string clientCode);
        Task<IEnumerable<RfidDto>> GetActiveRfidsAsync(string clientCode);
        Task<int> GetRfidCountByClientAsync(string clientCode);
        
        /// <summary>
        /// Get detailed analysis of used RFID tags (assigned to products) for a client
        /// </summary>
        /// <param name="clientCode">Client code to analyze</param>
        /// <returns>Used RFID analysis with count and detailed information</returns>
        Task<UsedRfidAnalysisDto> GetUsedRfidAnalysisAsync(string clientCode);
        
        /// <summary>
        /// Get detailed analysis of unused RFID tags (not assigned to products) for a client
        /// </summary>
        /// <param name="clientCode">Client code to analyze</param>
        /// <returns>Unused RFID analysis with count and detailed information</returns>
        Task<UnusedRfidAnalysisDto> GetUnusedRfidAnalysisAsync(string clientCode);

        /// <summary>
        /// Scan for products by EPC value - returns all products associated with the scanned EPC value(s)
        /// </summary>
        /// <param name="request">Scan request containing single EPC value or multiple EPC values</param>
        /// <param name="clientCode">Client code to search in</param>
        /// <returns>Scan response with all associated products grouped by EPC value</returns>
        Task<RfidScanResponseDto> ScanProductsByEpcValueAsync(RfidScanRequestDto request, string clientCode);
    }
} 