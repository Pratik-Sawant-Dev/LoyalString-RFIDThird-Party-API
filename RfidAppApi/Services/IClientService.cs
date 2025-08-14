using RfidAppApi.DTOs;
using RfidAppApi.Data;

namespace RfidAppApi.Services
{
    public interface IClientService
    {
        Task<ClientDbContext> GetClientDbContextAsync(string clientCode);
        Task<bool> IsValidClientAsync(string clientCode);
        Task<string> GetClientDatabaseNameAsync(string clientCode);
    }
} 