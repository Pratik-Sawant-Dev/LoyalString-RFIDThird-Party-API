using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IClientDatabaseService
    {
        /// <summary>
        /// Generates the next sequential client code (LS0001, LS0002, etc.)
        /// </summary>
        Task<string> GenerateClientCodeAsync();

        /// <summary>
        /// Creates a new client database with the given organization name and client code
        /// </summary>
        Task<string> CreateClientDatabaseAsync(string organisationName, string clientCode);

        /// <summary>
        /// Checks if a client database exists for the given client code
        /// </summary>
        Task<bool> ClientDatabaseExistsAsync(string clientCode);

        /// <summary>
        /// Gets the connection string for a client database
        /// </summary>
        Task<string> GetClientConnectionStringAsync(string clientCode);

        /// <summary>
        /// Gets all client codes from the master database
        /// </summary>
        Task<string[]> GetAllClientCodesAsync();
    }
} 