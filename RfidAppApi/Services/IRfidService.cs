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
    }
} 