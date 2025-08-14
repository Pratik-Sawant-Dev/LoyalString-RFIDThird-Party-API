using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RfidAppApi.Data;
using System.Data;

namespace RfidAppApi.Services
{
    public class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly IClientDatabaseService _clientDatabaseService;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(
            IClientDatabaseService clientDatabaseService,
            ILogger<DatabaseMigrationService> logger)
        {
            _clientDatabaseService = clientDatabaseService;
            _logger = logger;
        }

        /// <summary>
        /// Comprehensive migration for a single client database
        /// </summary>
        public async Task MigrateClientDatabaseAsync(string clientCode)
        {
            try
            {
                _logger.LogInformation("Starting comprehensive database migration for client: {ClientCode}", clientCode);

                // Get client database connection string
                var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogWarning("No connection string found for client: {ClientCode}", clientCode);
                    return;
                }

                // Create DbContext options for the client database
                var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new ClientDbContext(optionsBuilder.Options, clientCode);

                // Step 1: Ensure database exists
                await context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database ensured for client: {ClientCode}", clientCode);

                // Step 2: Apply any pending EF Core migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    _logger.LogInformation("Applying pending EF Core migrations for client: {ClientCode}", clientCode);
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Successfully applied EF Core migrations for client: {ClientCode}", clientCode);
                }

                // Step 3: Ensure all required custom tables exist (this is the key part)
                var tablesCreated = await EnsureAllRequiredTablesExistAsync(connectionString, clientCode);
                
                if (tablesCreated > 0)
                {
                    _logger.LogInformation("Created {TablesCreated} new tables for client: {ClientCode}", tablesCreated, clientCode);
                }
                else
                {
                    _logger.LogInformation("All required tables already exist for client: {ClientCode}", clientCode);
                }

                _logger.LogInformation("Successfully completed migration for client: {ClientCode}", clientCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating database for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Migrate all client databases with comprehensive table creation
        /// </summary>
        public async Task MigrateAllClientDatabasesAsync()
        {
            try
            {
                _logger.LogInformation("Starting comprehensive migration for all client databases");

                // Get all client codes from the main database
                var clientCodes = await _clientDatabaseService.GetAllClientCodesAsync();
                _logger.LogInformation("Found {ClientCount} clients to migrate", clientCodes.Length);

                var successCount = 0;
                var failureCount = 0;
                var totalTablesCreated = 0;

                foreach (var clientCode in clientCodes)
                {
                    try
                    {
                        await MigrateClientDatabaseAsync(clientCode);
                        successCount++;
                        _logger.LogInformation("Successfully migrated client: {ClientCode}", clientCode);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, "Failed to migrate database for client: {ClientCode}", clientCode);
                        // Continue with other clients even if one fails
                    }
                }

                _logger.LogInformation("Completed migration for all client databases. Success: {SuccessCount}, Failures: {FailureCount}", 
                    successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk database migration");
                throw;
            }
        }

        /// <summary>
        /// Ensure all required tables exist in the client database
        /// Returns the number of tables created
        /// </summary>
        private async Task<int> EnsureAllRequiredTablesExistAsync(string connectionString, string clientCode)
        {
            try
            {
                _logger.LogInformation("Ensuring all required tables exist for client: {ClientCode}", clientCode);

                // List of all required tables that need to be created
                var requiredTables = new[]
                {
                    "tblStockMovement",
                    "tblDailyStockBalance",
                    "tblProductImage"
                };

                var tablesCreated = 0;

                foreach (var tableName in requiredTables)
                {
                    try
                    {
                        if (!await TableExistsAsync(connectionString, tableName))
                        {
                            _logger.LogInformation("Creating missing table {TableName} for client: {ClientCode}", tableName, clientCode);
                            await CreateTableAsync(connectionString, tableName, clientCode);
                            tablesCreated++;
                            _logger.LogInformation("Successfully created table {TableName} for client: {ClientCode}", tableName, clientCode);
                        }
                        else
                        {
                            _logger.LogDebug("Table {TableName} already exists for client: {ClientCode}", tableName, clientCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating table {TableName} for client: {ClientCode}", tableName, clientCode);
                        throw; // Re-throw to stop the migration process
                    }
                }

                _logger.LogInformation("All required tables ensured for client: {ClientCode}. Tables created: {TablesCreated}", 
                    clientCode, tablesCreated);
                
                return tablesCreated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring required tables for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Create a specific table based on table name
        /// </summary>
        private async Task CreateTableAsync(string connectionString, string tableName, string clientCode)
        {
            try
            {
                string createTableSql = tableName switch
                {
                    "tblStockMovement" => GetStockMovementTableSql(),
                    "tblDailyStockBalance" => GetDailyStockBalanceTableSql(),
                    "tblProductImage" => GetProductImageTableSql(),
                    _ => throw new ArgumentException($"Unknown table name: {tableName}")
                };

                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new Microsoft.Data.SqlClient.SqlCommand(createTableSql, connection);
                command.CommandTimeout = 300; // 5 minutes timeout for large operations
                
                await command.ExecuteNonQueryAsync();
                
                _logger.LogInformation("Successfully created table {TableName} for client: {ClientCode}", tableName, clientCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating table {TableName} for client: {ClientCode}", tableName, clientCode);
                throw;
            }
        }

        /// <summary>
        /// Get SQL for creating StockMovement table
        /// </summary>
        private string GetStockMovementTableSql()
        {
            return @"
                CREATE TABLE [dbo].[tblStockMovement] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [ClientCode] NVARCHAR(50) NOT NULL,
                    [ProductId] INT NOT NULL,
                    [RfidCode] NVARCHAR(50) NULL,
                    [MovementType] NVARCHAR(20) NOT NULL,
                    [Quantity] INT NOT NULL DEFAULT(1),
                    [UnitPrice] DECIMAL(18,2) NULL,
                    [TotalAmount] DECIMAL(18,2) NULL,
                    [BranchId] INT NOT NULL,
                    [CounterId] INT NOT NULL,
                    [CategoryId] INT NOT NULL,
                    [ReferenceNumber] NVARCHAR(100) NULL,
                    [ReferenceType] NVARCHAR(50) NULL,
                    [Remarks] NVARCHAR(500) NULL,
                    [MovementDate] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [CreatedOn] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedOn] DATETIME2 NULL,
                    [IsActive] BIT NOT NULL DEFAULT(1),
                    CONSTRAINT [PK_tblStockMovement] PRIMARY KEY ([Id])
                );

                -- Create indexes for optimal performance
                CREATE INDEX [IX_tblStockMovement_ProductId] ON [dbo].[tblStockMovement] ([ProductId]);
                CREATE INDEX [IX_tblStockMovement_MovementType] ON [dbo].[tblStockMovement] ([MovementType]);
                CREATE INDEX [IX_tblStockMovement_MovementDate] ON [dbo].[tblStockMovement] ([MovementDate]);
                CREATE INDEX [IX_tblStockMovement_CreatedOn] ON [dbo].[tblStockMovement] ([CreatedOn]);
                CREATE INDEX [IX_tblStockMovement_BranchId] ON [dbo].[tblStockMovement] ([BranchId]);
                CREATE INDEX [IX_tblStockMovement_CounterId] ON [dbo].[tblStockMovement] ([CounterId]);
                CREATE INDEX [IX_tblStockMovement_CategoryId] ON [dbo].[tblStockMovement] ([CategoryId]);
                CREATE INDEX [IX_tblStockMovement_RfidCode] ON [dbo].[tblStockMovement] ([RfidCode]);
                CREATE INDEX [IX_tblStockMovement_ReferenceNumber] ON [dbo].[tblStockMovement] ([ReferenceNumber]);
                CREATE INDEX [IX_tblStockMovement_ProductId_MovementDate] ON [dbo].[tblStockMovement] ([ProductId], [MovementDate]);
                CREATE INDEX [IX_tblStockMovement_MovementType_MovementDate] ON [dbo].[tblStockMovement] ([MovementType], [MovementDate]);
                CREATE INDEX [IX_tblStockMovement_BranchId_MovementDate] ON [dbo].[tblStockMovement] ([BranchId], [MovementDate]);
                CREATE INDEX [IX_tblStockMovement_CounterId_MovementDate] ON [dbo].[tblStockMovement] ([CounterId], [MovementDate]);
                CREATE INDEX [IX_tblStockMovement_CategoryId_MovementDate] ON [dbo].[tblStockMovement] ([CategoryId], [MovementDate]);
                CREATE INDEX [IX_tblStockMovement_ClientCode_MovementDate] ON [dbo].[tblStockMovement] ([ClientCode], [MovementDate]);

                -- Add foreign key constraints (only if referenced tables exist)
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblProductDetails]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblStockMovement] 
                    ADD CONSTRAINT [FK_tblStockMovement_tblProductDetails] 
                    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tblProductDetails]([Id]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblBranchMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblStockMovement] 
                    ADD CONSTRAINT [FK_tblStockMovement_tblBranchMaster] 
                    FOREIGN KEY ([BranchId]) REFERENCES [dbo].[tblBranchMaster]([BranchId]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblCounterMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblStockMovement] 
                    ADD CONSTRAINT [FK_tblStockMovement_tblCounterMaster] 
                    FOREIGN KEY ([CounterId]) REFERENCES [dbo].[tblCounterMaster]([CounterId]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblCategoryMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblStockMovement] 
                    ADD CONSTRAINT [FK_tblStockMovement_tblCategoryMaster] 
                    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[tblCategoryMaster]([CategoryId]) 
                    ON DELETE RESTRICT;
                END";
        }

        /// <summary>
        /// Get SQL for creating DailyStockBalance table
        /// </summary>
        private string GetDailyStockBalanceTableSql()
        {
            return @"
                CREATE TABLE [dbo].[tblDailyStockBalance] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [ClientCode] NVARCHAR(50) NOT NULL,
                    [ProductId] INT NOT NULL,
                    [RfidCode] NVARCHAR(50) NULL,
                    [BranchId] INT NOT NULL,
                    [CounterId] INT NOT NULL,
                    [CategoryId] INT NOT NULL,
                    [BalanceDate] DATETIME2 NOT NULL,
                    [OpeningQuantity] INT NOT NULL DEFAULT(0),
                    [ClosingQuantity] INT NOT NULL DEFAULT(0),
                    [AddedQuantity] INT NOT NULL DEFAULT(0),
                    [SoldQuantity] INT NOT NULL DEFAULT(0),
                    [ReturnedQuantity] INT NOT NULL DEFAULT(0),
                    [TransferredInQuantity] INT NOT NULL DEFAULT(0),
                    [TransferredOutQuantity] INT NOT NULL DEFAULT(0),
                    [OpeningValue] DECIMAL(18,2) NULL,
                    [ClosingValue] DECIMAL(18,2) NULL,
                    [AddedValue] DECIMAL(18,2) NULL,
                    [SoldValue] DECIMAL(18,2) NULL,
                    [ReturnedValue] DECIMAL(18,2) NULL,
                    [CreatedOn] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedOn] DATETIME2 NULL,
                    [IsActive] BIT NOT NULL DEFAULT(1),
                    CONSTRAINT [PK_tblDailyStockBalance] PRIMARY KEY ([Id])
                );

                -- Create indexes for optimal performance
                CREATE INDEX [IX_tblDailyStockBalance_ProductId] ON [dbo].[tblDailyStockBalance] ([ProductId]);
                CREATE INDEX [IX_tblDailyStockBalance_BalanceDate] ON [dbo].[tblDailyStockBalance] ([BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_CreatedOn] ON [dbo].[tblDailyStockBalance] ([CreatedOn]);
                CREATE INDEX [IX_tblDailyStockBalance_BranchId] ON [dbo].[tblDailyStockBalance] ([BranchId]);
                CREATE INDEX [IX_tblDailyStockBalance_CounterId] ON [dbo].[tblDailyStockBalance] ([CounterId]);
                CREATE INDEX [IX_tblDailyStockBalance_CategoryId] ON [dbo].[tblDailyStockBalance] ([CategoryId]);
                CREATE INDEX [IX_tblDailyStockBalance_RfidCode] ON [dbo].[tblDailyStockBalance] ([RfidCode]);
                CREATE UNIQUE INDEX [IX_tblDailyStockBalance_ProductId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([ProductId], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_BranchId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([BranchId], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_CounterId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([CounterId], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_CategoryId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([CategoryId], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_ClientCode_BalanceDate] ON [dbo].[tblDailyStockBalance] ([ClientCode], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_ProductId_BranchId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([ProductId], [BranchId], [BalanceDate]);
                CREATE INDEX [IX_tblDailyStockBalance_ProductId_CounterId_BalanceDate] ON [dbo].[tblDailyStockBalance] ([ProductId], [CounterId], [BalanceDate]);

                -- Add foreign key constraints (only if referenced tables exist)
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblProductDetails]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblDailyStockBalance] 
                    ADD CONSTRAINT [FK_tblDailyStockBalance_tblProductDetails] 
                    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tblProductDetails]([Id]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblBranchMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblDailyStockBalance] 
                    ADD CONSTRAINT [FK_tblDailyStockBalance_tblBranchMaster] 
                    FOREIGN KEY ([BranchId]) REFERENCES [dbo].[tblBranchMaster]([BranchId]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblCounterMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblDailyStockBalance] 
                    ADD CONSTRAINT [FK_tblDailyStockBalance_tblCounterMaster] 
                    FOREIGN KEY ([CounterId]) REFERENCES [dbo].[tblCounterMaster]([CounterId]) 
                    ON DELETE RESTRICT;
                END

                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblCategoryMaster]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblDailyStockBalance] 
                    ADD CONSTRAINT [FK_tblDailyStockBalance_tblCategoryMaster] 
                    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[tblCategoryMaster]([CategoryId]) 
                    ON DELETE RESTRICT;
                END";
        }

        /// <summary>
        /// Get SQL for creating ProductImage table
        /// </summary>
        private string GetProductImageTableSql()
        {
            return @"
                CREATE TABLE [dbo].[tblProductImage] (
                    [Id] INT IDENTITY(1,1) NOT NULL,
                    [ClientCode] NVARCHAR(50) NOT NULL,
                    [ProductId] INT NOT NULL,
                    [ImageFileName] NVARCHAR(255) NOT NULL,
                    [ImagePath] NVARCHAR(500) NOT NULL,
                    [ImageType] NVARCHAR(50) NULL,
                    [ImageSize] BIGINT NULL,
                    [IsPrimary] BIT NOT NULL DEFAULT(0),
                    [SortOrder] INT NOT NULL DEFAULT(0),
                    [CreatedOn] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
                    [UpdatedOn] DATETIME2 NULL,
                    [IsActive] BIT NOT NULL DEFAULT(1),
                    CONSTRAINT [PK_tblProductImage] PRIMARY KEY ([Id])
                );

                -- Create indexes for optimal performance
                CREATE INDEX [IX_tblProductImage_ProductId] ON [dbo].[tblProductImage] ([ProductId]);
                CREATE INDEX [IX_tblProductImage_ClientCode] ON [dbo].[tblProductImage] ([ClientCode]);
                CREATE INDEX [IX_tblProductImage_IsPrimary] ON [dbo].[tblProductImage] ([IsPrimary]);
                CREATE INDEX [IX_tblProductImage_SortOrder] ON [dbo].[tblProductImage] ([SortOrder]);
                CREATE INDEX [IX_tblProductImage_CreatedOn] ON [dbo].[tblProductImage] ([CreatedOn]);

                -- Add foreign key constraint (only if referenced table exists)
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblProductDetails]') AND type in (N'U'))
                BEGIN
                    ALTER TABLE [dbo].[tblProductImage] 
                    ADD CONSTRAINT [FK_tblProductImage_tblProductDetails] 
                    FOREIGN KEY ([ProductId]) REFERENCES [dbo].[tblProductDetails]([Id]) 
                    ON DELETE CASCADE;
                END";
        }

        /// <summary>
        /// Check if a table exists in the database
        /// </summary>
        private async Task<bool> TableExistsAsync(string connectionString, string tableName)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' 
                    AND TABLE_NAME = @TableName";
                
                using var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if table {TableName} exists", tableName);
                return false;
            }
        }

        /// <summary>
        /// Check if database is up to date
        /// </summary>
        public async Task<bool> IsDatabaseUpToDateAsync(string clientCode)
        {
            try
            {
                var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
                if (string.IsNullOrEmpty(connectionString))
                    return false;

                var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new ClientDbContext(optionsBuilder.Options, clientCode);

                // Check if database exists and can connect
                if (!await context.Database.CanConnectAsync())
                    return false;

                // Check if there are any pending EF Core migrations
                var pendingMigrations = context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                    return false;

                // Check if all required custom tables exist
                var requiredTables = new[] { "tblStockMovement", "tblDailyStockBalance", "tblProductImage" };
                foreach (var tableName in requiredTables)
                {
                    if (!await TableExistsAsync(connectionString, tableName))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database status for client: {ClientCode}", clientCode);
                return false;
            }
        }

        /// <summary>
        /// Get pending migrations for a client
        /// </summary>
        public async Task<string[]> GetPendingMigrationsAsync(string clientCode)
        {
            try
            {
                var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
                if (string.IsNullOrEmpty(connectionString))
                    return Array.Empty<string>();

                var optionsBuilder = new DbContextOptionsBuilder<ClientDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new ClientDbContext(optionsBuilder.Options, clientCode);

                var pendingMigrations = new List<string>();

                // Check EF Core migrations
                var efMigrations = context.Database.GetPendingMigrations();
                pendingMigrations.AddRange(efMigrations);

                // Check for missing required custom tables
                var requiredTables = new[] { "tblStockMovement", "tblDailyStockBalance", "tblProductImage" };
                foreach (var tableName in requiredTables)
                {
                    if (!await TableExistsAsync(connectionString, tableName))
                    {
                        pendingMigrations.Add($"Missing table: {tableName}");
                    }
                }

                return pendingMigrations.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending migrations for client: {ClientCode}", clientCode);
                return Array.Empty<string>();
            }
        }

        // Legacy methods for backward compatibility
        public async Task AddProductImageTableToClientAsync(string clientCode)
        {
            await MigrateClientDatabaseAsync(clientCode);
        }

        public async Task AddProductImageTableToAllClientsAsync()
        {
            await MigrateAllClientDatabasesAsync();
        }

        public async Task AddReportingTablesToClientAsync(string clientCode)
        {
            await MigrateClientDatabaseAsync(clientCode);
        }

        public async Task AddReportingTablesToAllClientsAsync()
        {
            await MigrateAllClientDatabasesAsync();
        }
    }
}
