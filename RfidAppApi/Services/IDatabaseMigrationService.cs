namespace RfidAppApi.Services
{
    public interface IDatabaseMigrationService
    {
        Task MigrateClientDatabaseAsync(string clientCode);
        Task MigrateAllClientDatabasesAsync();
        Task<bool> IsDatabaseUpToDateAsync(string clientCode);
        Task<string[]> GetPendingMigrationsAsync(string clientCode);
        
        /// <summary>
        /// Add ProductImage table to existing client databases
        /// </summary>
        Task AddProductImageTableToClientAsync(string clientCode);
        
        /// <summary>
        /// Add ProductImage table to all existing client databases
        /// </summary>
        Task AddProductImageTableToAllClientsAsync();
        
        /// <summary>
        /// Add reporting tables to existing client database
        /// </summary>
        Task AddReportingTablesToClientAsync(string clientCode);
        
        /// <summary>
        /// Add reporting tables to all existing client databases
        /// </summary>
        Task AddReportingTablesToAllClientsAsync();
    }
}
