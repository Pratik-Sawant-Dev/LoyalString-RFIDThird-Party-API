using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for database migration and health monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class DatabaseMigrationController : ControllerBase
    {
        private readonly IDatabaseMigrationService _migrationService;
        private readonly IClientDatabaseService _clientDatabaseService;
        private readonly ILogger<DatabaseMigrationController> _logger;

        public DatabaseMigrationController(
            IDatabaseMigrationService migrationService,
            IClientDatabaseService clientDatabaseService,
            ILogger<DatabaseMigrationController> logger)
        {
            _migrationService = migrationService;
            _clientDatabaseService = clientDatabaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get database health status for all clients
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<object>> GetDatabaseHealth()
        {
            try
            {
                var clientCodes = await _clientDatabaseService.GetAllClientCodesAsync();
                var healthReport = new List<object>();

                foreach (var clientCode in clientCodes)
                {
                    try
                    {
                        var isUpToDate = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                        var pendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                        healthReport.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = isUpToDate,
                            PendingMigrations = pendingMigrations,
                            Status = isUpToDate ? "Healthy" : "Needs Migration"
                        });
                    }
                    catch (Exception ex)
                    {
                        healthReport.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = false,
                            PendingMigrations = new[] { $"Error: {ex.Message}" },
                            Status = "Error"
                        });
                    }
                }

                return Ok(new
                {
                    TotalClients = clientCodes.Length,
                    HealthyClients = healthReport.Count(h => (bool)h.GetType().GetProperty("IsHealthy").GetValue(h)),
                    UnhealthyClients = healthReport.Count(h => !(bool)h.GetType().GetProperty("IsHealthy").GetValue(h)),
                    Details = healthReport
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database health status");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get database health status for a specific client
        /// </summary>
        [HttpGet("health/{clientCode}")]
        public async Task<ActionResult<object>> GetClientDatabaseHealth(string clientCode)
        {
            try
            {
                var isUpToDate = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                var pendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                return Ok(new
                {
                    ClientCode = clientCode,
                    IsHealthy = isUpToDate,
                    PendingMigrations = pendingMigrations,
                    Status = isUpToDate ? "Healthy" : "Needs Migration",
                    LastChecked = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database health status for client: {ClientCode}", clientCode);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrate a specific client database
        /// </summary>
        [HttpPost("migrate/{clientCode}")]
        public async Task<ActionResult<object>> MigrateClientDatabase(string clientCode)
        {
            try
            {
                _logger.LogInformation("Starting manual migration for client: {ClientCode}", clientCode);

                var startTime = DateTime.UtcNow;
                await _migrationService.MigrateClientDatabaseAsync(clientCode);
                var endTime = DateTime.UtcNow;

                // Verify migration was successful
                var isUpToDate = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                var pendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                return Ok(new
                {
                    ClientCode = clientCode,
                    Success = true,
                    MigrationTime = endTime - startTime,
                    IsHealthy = isUpToDate,
                    PendingMigrations = pendingMigrations,
                    Status = isUpToDate ? "Healthy" : "Needs Attention",
                    Message = "Database migration completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating database for client: {ClientCode}", clientCode);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrate all client databases
        /// </summary>
        [HttpPost("migrate-all")]
        public async Task<ActionResult<object>> MigrateAllClientDatabases()
        {
            try
            {
                _logger.LogInformation("Starting manual migration for all client databases");

                var startTime = DateTime.UtcNow;
                await _migrationService.MigrateAllClientDatabasesAsync();
                var endTime = DateTime.UtcNow;

                // Get health status after migration
                var clientCodes = await _clientDatabaseService.GetAllClientCodesAsync();
                var healthReport = new List<object>();

                foreach (var clientCode in clientCodes)
                {
                    try
                    {
                        var isUpToDate = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                        var pendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                        healthReport.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = isUpToDate,
                            PendingMigrations = pendingMigrations,
                            Status = isUpToDate ? "Healthy" : "Needs Attention"
                        });
                    }
                    catch (Exception ex)
                    {
                        healthReport.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = false,
                            PendingMigrations = new[] { $"Error: {ex.Message}" },
                            Status = "Error"
                        });
                    }
                }

                return Ok(new
                {
                    Success = true,
                    TotalMigrationTime = endTime - startTime,
                    TotalClients = clientCodes.Length,
                    HealthyClients = healthReport.Count(h => (bool)h.GetType().GetProperty("IsHealthy").GetValue(h)),
                    UnhealthyClients = healthReport.Count(h => !(bool)h.GetType().GetProperty("IsHealthy").GetValue(h)),
                    Message = "Bulk database migration completed",
                    Details = healthReport
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk database migration");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Force repair a specific client database
        /// </summary>
        [HttpPost("repair/{clientCode}")]
        public async Task<ActionResult<object>> RepairClientDatabase(string clientCode)
        {
            try
            {
                _logger.LogInformation("Starting database repair for client: {ClientCode}", clientCode);

                var startTime = DateTime.UtcNow;

                // First, check current status
                var initialStatus = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                var initialPendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                // Perform migration
                await _migrationService.MigrateClientDatabaseAsync(clientCode);

                // Check status after repair
                var finalStatus = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                var finalPendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                var endTime = DateTime.UtcNow;

                return Ok(new
                {
                    ClientCode = clientCode,
                    Success = true,
                    RepairTime = endTime - startTime,
                    InitialStatus = new
                    {
                        IsHealthy = initialStatus,
                        PendingMigrations = initialPendingMigrations
                    },
                    FinalStatus = new
                    {
                        IsHealthy = finalStatus,
                        PendingMigrations = finalPendingMigrations
                    },
                    Repaired = !initialStatus && finalStatus,
                    Message = finalStatus ? "Database repaired successfully" : "Database repair completed but issues remain"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error repairing database for client: {ClientCode}", clientCode);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get migration statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetMigrationStatistics()
        {
            try
            {
                var clientCodes = await _clientDatabaseService.GetAllClientCodesAsync();
                var statistics = new List<object>();

                foreach (var clientCode in clientCodes)
                {
                    try
                    {
                        var isUpToDate = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                        var pendingMigrations = await _migrationService.GetPendingMigrationsAsync(clientCode);

                        statistics.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = isUpToDate,
                            PendingMigrationsCount = pendingMigrations.Length,
                            PendingMigrations = pendingMigrations,
                            LastChecked = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        statistics.Add(new
                        {
                            ClientCode = clientCode,
                            IsHealthy = false,
                            PendingMigrationsCount = -1,
                            PendingMigrations = new[] { $"Error: {ex.Message}" },
                            LastChecked = DateTime.UtcNow
                        });
                    }
                }

                var totalClients = clientCodes.Length;
                var healthyClients = statistics.Count(s => (bool)s.GetType().GetProperty("IsHealthy").GetValue(s));
                var unhealthyClients = totalClients - healthyClients;

                return Ok(new
                {
                    Summary = new
                    {
                        TotalClients = totalClients,
                        HealthyClients = healthyClients,
                        UnhealthyClients = unhealthyClients,
                        HealthPercentage = totalClients > 0 ? (double)healthyClients / totalClients * 100 : 0
                    },
                    Details = statistics,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration statistics");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Emergency database repair for all clients
        /// </summary>
        [HttpPost("emergency-repair-all")]
        public async Task<ActionResult<object>> EmergencyRepairAllDatabases()
        {
            try
            {
                _logger.LogWarning("Starting emergency database repair for all clients");

                var startTime = DateTime.UtcNow;
                var clientCodes = await _clientDatabaseService.GetAllClientCodesAsync();
                var repairResults = new List<object>();

                foreach (var clientCode in clientCodes)
                {
                    try
                    {
                        var clientStartTime = DateTime.UtcNow;
                        
                        // Check initial status
                        var initialStatus = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                        
                        // Perform migration
                        await _migrationService.MigrateClientDatabaseAsync(clientCode);
                        
                        // Check final status
                        var finalStatus = await _migrationService.IsDatabaseUpToDateAsync(clientCode);
                        var clientEndTime = DateTime.UtcNow;

                        repairResults.Add(new
                        {
                            ClientCode = clientCode,
                            Success = true,
                            InitialStatus = initialStatus,
                            FinalStatus = finalStatus,
                            RepairTime = clientEndTime - clientStartTime,
                            Repaired = !initialStatus && finalStatus
                        });
                    }
                    catch (Exception ex)
                    {
                        repairResults.Add(new
                        {
                            ClientCode = clientCode,
                            Success = false,
                            Error = ex.Message,
                            RepairTime = TimeSpan.Zero
                        });
                    }
                }

                var endTime = DateTime.UtcNow;
                var successfulRepairs = repairResults.Count(r => (bool)r.GetType().GetProperty("Success").GetValue(r));
                var failedRepairs = repairResults.Count - successfulRepairs;

                return Ok(new
                {
                    Success = true,
                    TotalRepairTime = endTime - startTime,
                    TotalClients = clientCodes.Length,
                    SuccessfulRepairs = successfulRepairs,
                    FailedRepairs = failedRepairs,
                    Message = "Emergency database repair completed",
                    Details = repairResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during emergency database repair");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Test endpoint to verify table creation for a specific client
        /// </summary>
        [HttpGet("test-tables/{clientCode}")]
        public async Task<ActionResult<object>> TestTableCreation(string clientCode)
        {
            try
            {
                var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest($"No connection string found for client: {clientCode}");
                }

                var requiredTables = new[] { "tblStockMovement", "tblDailyStockBalance", "tblProductImage" };
                var tableStatus = new List<object>();

                foreach (var tableName in requiredTables)
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
                        var exists = Convert.ToInt32(count) > 0;

                        tableStatus.Add(new
                        {
                            TableName = tableName,
                            Exists = exists,
                            Status = exists ? "OK" : "Missing"
                        });
                    }
                    catch (Exception ex)
                    {
                        tableStatus.Add(new
                        {
                            TableName = tableName,
                            Exists = false,
                            Status = $"Error: {ex.Message}"
                        });
                    }
                }

                var missingTables = tableStatus.Count(s => !(bool)s.GetType().GetProperty("Exists").GetValue(s));

                return Ok(new
                {
                    ClientCode = clientCode,
                    TotalTables = requiredTables.Length,
                    ExistingTables = requiredTables.Length - missingTables,
                    MissingTables = missingTables,
                    TableStatus = tableStatus,
                    TestedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing table creation for client: {ClientCode}", clientCode);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Force create missing tables for a specific client
        /// </summary>
        [HttpPost("force-create-tables/{clientCode}")]
        public async Task<ActionResult<object>> ForceCreateMissingTables(string clientCode)
        {
            try
            {
                _logger.LogInformation("Force creating missing tables for client: {ClientCode}", clientCode);

                var startTime = DateTime.UtcNow;
                var connectionString = await _clientDatabaseService.GetClientConnectionStringAsync(clientCode);
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest($"No connection string found for client: {clientCode}");
                }

                var requiredTables = new[] { "tblStockMovement", "tblDailyStockBalance", "tblProductImage" };
                var createdTables = new List<string>();
                var existingTables = new List<string>();

                foreach (var tableName in requiredTables)
                {
                    try
                    {
                        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                        await connection.OpenAsync();
                        
                        // Check if table exists
                        var checkSql = @"
                            SELECT COUNT(*) 
                            FROM INFORMATION_SCHEMA.TABLES 
                            WHERE TABLE_SCHEMA = 'dbo' 
                            AND TABLE_NAME = @TableName";
                        
                        using var checkCommand = new Microsoft.Data.SqlClient.SqlCommand(checkSql, connection);
                        checkCommand.Parameters.AddWithValue("@TableName", tableName);
                        
                        var count = await checkCommand.ExecuteScalarAsync();
                        var exists = Convert.ToInt32(count) > 0;

                        if (!exists)
                        {
                            // Create table
                            string createTableSql = tableName switch
                            {
                                "tblStockMovement" => GetStockMovementTableSql(),
                                "tblDailyStockBalance" => GetDailyStockBalanceTableSql(),
                                "tblProductImage" => GetProductImageTableSql(),
                                _ => throw new ArgumentException($"Unknown table name: {tableName}")
                            };

                            using var createCommand = new Microsoft.Data.SqlClient.SqlCommand(createTableSql, connection);
                            createCommand.CommandTimeout = 300;
                            await createCommand.ExecuteNonQueryAsync();
                            
                            createdTables.Add(tableName);
                            _logger.LogInformation("Successfully created table {TableName} for client: {ClientCode}", tableName, clientCode);
                        }
                        else
                        {
                            existingTables.Add(tableName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating table {TableName} for client: {ClientCode}", tableName, clientCode);
                        return StatusCode(500, $"Error creating table {tableName}: {ex.Message}");
                    }
                }

                var endTime = DateTime.UtcNow;

                return Ok(new
                {
                    ClientCode = clientCode,
                    Success = true,
                    OperationTime = endTime - startTime,
                    TablesCreated = createdTables,
                    TablesExisting = existingTables,
                    TotalCreated = createdTables.Count,
                    TotalExisting = existingTables.Count,
                    Message = $"Force create tables completed. Created: {createdTables.Count}, Existing: {existingTables.Count}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force creating tables for client: {ClientCode}", clientCode);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper methods for table creation SQL
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
                )";
        }

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
                )";
        }

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
                )";
        }
    }
}
