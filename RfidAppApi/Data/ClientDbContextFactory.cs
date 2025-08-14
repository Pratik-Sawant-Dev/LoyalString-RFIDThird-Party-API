using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RfidAppApi.Services;

namespace RfidAppApi.Data
{
    public class ClientDbContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IClientDatabaseService _clientDatabaseService;

        public ClientDbContextFactory(IConfiguration configuration, IClientDatabaseService clientDatabaseService)
        {
            _configuration = configuration;
            _clientDatabaseService = clientDatabaseService;
        }

        public async Task<ClientDbContext> CreateAsync(string clientCode)
        {
            // Get the client-specific connection string
            var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
            
            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            // Create and return the ClientDbContext with the client code
            return new ClientDbContext(optionsBuilder.Options, clientCode);
        }

        public ClientDbContext Create(string clientCode, string connectionString)
        {
            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            // Create and return the ClientDbContext with the client code
            return new ClientDbContext(optionsBuilder.Options, clientCode);
        }
    }
} 