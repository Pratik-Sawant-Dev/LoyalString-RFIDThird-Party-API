using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RfidAppApi.Data;
using RfidAppApi.Models;
using System.Data.SqlClient;

namespace RfidAppApi.Services
{
    public class ClientDatabaseService : IClientDatabaseService
    {
        private readonly AppDbContext _masterContext;
        private readonly IConfiguration _configuration;

        public ClientDatabaseService(AppDbContext masterContext, IConfiguration configuration)
        {
            _masterContext = masterContext;
            _configuration = configuration;
        }

        public async Task<string> GenerateClientCodeAsync()
        {
            // Get the last client code from master database
            var lastUser = await _masterContext.Users
                .OrderByDescending(u => u.ClientCode)
                .FirstOrDefaultAsync();

            if (lastUser == null)
            {
                // First client - start with LS0001
                return "LS0001";
            }

            // Extract number from last client code (e.g., "LS0001" -> 1)
            if (lastUser.ClientCode.StartsWith("LS") && lastUser.ClientCode.Length == 6)
            {
                if (int.TryParse(lastUser.ClientCode.Substring(2), out int lastNumber))
                {
                    // Generate next sequential number
                    int nextNumber = lastNumber + 1;
                    return $"LS{nextNumber:D4}"; // Format as LS0002, LS0003, etc.
                }
            }

            // Fallback - if parsing fails, start from LS0001
            return "LS0001";
        }

        public async Task<string> CreateClientDatabaseAsync(string organisationName, string clientCode)
        {
            // Create database name: RFID_LS0001_OrganisationName
            var databaseName = $"RFID_{clientCode}_{organisationName.Replace(" ", "_").Replace("-", "_")}";

            // Get master database connection string
            var masterConnectionString = _configuration.GetConnectionString("DefaultConnection");
            
            // Create new connection string for client database
            var clientConnectionString = masterConnectionString.Replace("Database=RfidJewelryDB", $"Database={databaseName}");

            // Create database using SQL
            using (var connection = new SqlConnection(masterConnectionString))
            {
                await connection.OpenAsync();
                
                // Create database
                var createDbCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
                await createDbCommand.ExecuteNonQueryAsync();
            }

            // Initialize the new database with schema and seed data
            await InitializeClientDatabaseAsync(databaseName, clientCode, clientConnectionString);

            return databaseName;
        }

        public async Task<bool> ClientDatabaseExistsAsync(string clientCode)
        {
            var user = await _masterContext.Users
                .FirstOrDefaultAsync(u => u.ClientCode == clientCode);

            if (user?.DatabaseName == null) return false;

            var masterConnectionString = _configuration.GetConnectionString("DefaultConnection");
            
            using (var connection = new SqlConnection(masterConnectionString))
            {
                await connection.OpenAsync();
                
                var command = new SqlCommand(
                    "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName", 
                    connection);
                command.Parameters.AddWithValue("@DatabaseName", user.DatabaseName);
                
                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<string> GetClientConnectionStringAsync(string clientCode)
        {
            var user = await _masterContext.Users
                .FirstOrDefaultAsync(u => u.ClientCode == clientCode);

            if (user?.DatabaseName == null)
                throw new InvalidOperationException($"Client database not found for code: {clientCode}");

            var masterConnectionString = _configuration.GetConnectionString("DefaultConnection");
            return masterConnectionString.Replace("Database=RfidJewelryDB", $"Database={user.DatabaseName}");
        }

        private async Task InitializeClientDatabaseAsync(string databaseName, string clientCode, string clientConnectionString)
        {
            // Create DbContext options for client database
            var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
            optionsBuilder.UseSqlServer(clientConnectionString);

            using (var clientContext = new ClientDbContext(optionsBuilder.Options, clientCode))
            {
                // Create all tables
                await clientContext.Database.EnsureCreatedAsync();

                // Seed initial master data
                await SeedClientDatabaseAsync(clientContext, clientCode);
            }
        }

        private async Task SeedClientDatabaseAsync(ClientDbContext context, string clientCode)
        {
            // Seed Category Master Data
            if (!await context.CategoryMasters.AnyAsync())
            {
                var categories = new[]
                {
                    new CategoryMaster { CategoryName = "Rings" },
                    new CategoryMaster { CategoryName = "Necklaces" },
                    new CategoryMaster { CategoryName = "Earrings" },
                    new CategoryMaster { CategoryName = "Bracelets" },
                    new CategoryMaster { CategoryName = "Pendants" }
                };
                await context.CategoryMasters.AddRangeAsync(categories);
            }

            // Seed Product Master Data
            if (!await context.ProductMasters.AnyAsync())
            {
                var products = new[]
                {
                    new ProductMaster { ProductName = "Gold Ring" },
                    new ProductMaster { ProductName = "Silver Necklace" },
                    new ProductMaster { ProductName = "Diamond Earrings" },
                    new ProductMaster { ProductName = "Platinum Bracelet" }
                };
                await context.ProductMasters.AddRangeAsync(products);
            }

            // Seed Design Master Data
            if (!await context.DesignMasters.AnyAsync())
            {
                var designs = new[]
                {
                    new DesignMaster { DesignName = "Classic" },
                    new DesignMaster { DesignName = "Modern" },
                    new DesignMaster { DesignName = "Traditional" },
                    new DesignMaster { DesignName = "Contemporary" }
                };
                await context.DesignMasters.AddRangeAsync(designs);
            }

            // Seed Purity Master Data
            if (!await context.PurityMasters.AnyAsync())
            {
                var purities = new[]
                {
                    new PurityMaster { PurityName = "24K" },
                    new PurityMaster { PurityName = "22K" },
                    new PurityMaster { PurityName = "18K" },
                    new PurityMaster { PurityName = "14K" },
                    new PurityMaster { PurityName = "925 Silver" }
                };
                await context.PurityMasters.AddRangeAsync(purities);
            }

            // Seed Branch Master Data
            if (!await context.BranchMasters.AnyAsync())
            {
                var branches = new[]
                {
                    new BranchMaster { BranchName = "Main Branch", ClientCode = clientCode },
                    new BranchMaster { BranchName = "North Branch", ClientCode = clientCode },
                    new BranchMaster { BranchName = "South Branch", ClientCode = clientCode }
                };
                await context.BranchMasters.AddRangeAsync(branches);
            }

            // Seed Counter Master Data
            if (!await context.CounterMasters.AnyAsync())
            {
                var counters = new[]
                {
                    new CounterMaster { CounterName = "Counter 1", BranchId = 1 },
                    new CounterMaster { CounterName = "Counter 2", BranchId = 1 },
                    new CounterMaster { CounterName = "Counter 1", BranchId = 2 },
                    new CounterMaster { CounterName = "Counter 1", BranchId = 3 }
                };
                await context.CounterMasters.AddRangeAsync(counters);
            }

            await context.SaveChangesAsync();
        }

        public async Task<string[]> GetAllClientCodesAsync()
        {
            var clientCodes = await _masterContext.Users
                .Where(u => !string.IsNullOrEmpty(u.ClientCode))
                .Select(u => u.ClientCode)
                .ToArrayAsync();

            return clientCodes;
        }
    }
} 