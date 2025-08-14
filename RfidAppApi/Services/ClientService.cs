using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RfidAppApi.Data;

namespace RfidAppApi.Services
{
    public class ClientService : IClientService
    {
        private readonly IConfiguration _configuration;
        private readonly IClientDatabaseService _clientDatabaseService;
        private readonly AppDbContext _masterDbContext;

        public ClientService(
            IConfiguration configuration, 
            IClientDatabaseService clientDatabaseService,
            AppDbContext masterDbContext)
        {
            _configuration = configuration;
            _clientDatabaseService = clientDatabaseService;
            _masterDbContext = masterDbContext;
        }

        public async Task<ClientDbContext> GetClientDbContextAsync(string clientCode)
        {
            if (!await IsValidClientAsync(clientCode))
            {
                throw new InvalidOperationException($"Invalid client code: {clientCode}");
            }

            var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
            
            var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ClientDbContext(optionsBuilder.Options, clientCode);
        }

        public async Task<bool> IsValidClientAsync(string clientCode)
        {
            var user = await _masterDbContext.Users
                .FirstOrDefaultAsync(u => u.ClientCode == clientCode && u.IsActive);

            return user != null && !string.IsNullOrEmpty(user.DatabaseName);
        }

        public async Task<string> GetClientDatabaseNameAsync(string clientCode)
        {
            var user = await _masterDbContext.Users
                .FirstOrDefaultAsync(u => u.ClientCode == clientCode);

            return user?.DatabaseName ?? string.Empty;
        }
    }
} 