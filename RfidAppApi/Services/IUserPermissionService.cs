using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service interface for user permission management
    /// </summary>
    public interface IUserPermissionService
    {
        /// <summary>
        /// Get user permissions by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user permissions</returns>
        Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId);

        /// <summary>
        /// Get user permission summary
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User permission summary</returns>
        Task<UserPermissionSummaryDto> GetUserPermissionSummaryAsync(int userId);

        /// <summary>
        /// Get user access information including branch and counter access
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User access information</returns>
        Task<UserAccessInfoDto> GetUserAccessInfoAsync(int userId);

        /// <summary>
        /// Check if user has specific permission for a module
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="module">Module name</param>
        /// <param name="action">Action (view, create, edit, delete, export, import)</param>
        /// <returns>True if user has permission</returns>
        Task<bool> HasPermissionAsync(int userId, string module, string action);

        /// <summary>
        /// Get all available permission modules
        /// </summary>
        /// <returns>List of available modules</returns>
        Task<IEnumerable<string>> GetAvailableModulesAsync();

        /// <summary>
        /// Get user's accessible branch IDs
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of accessible branch IDs</returns>
        Task<List<int>> GetAccessibleBranchIdsAsync(int userId);

        /// <summary>
        /// Get user's accessible counter IDs
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of accessible counter IDs</returns>
        Task<List<int>> GetAccessibleCounterIdsAsync(int userId);

        /// <summary>
        /// Get comprehensive user permission details for login
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Complete user permission details</returns>
        Task<UserPermissionDetailsDto> GetUserPermissionDetailsAsync(int userId);
    }

    /// <summary>
    /// Data transfer object for comprehensive user permission details
    /// </summary>
    public class UserPermissionDetailsDto
    {
        public List<UserPermissionDto> Permissions { get; set; } = new List<UserPermissionDto>();
        public UserPermissionSummaryDto PermissionSummary { get; set; } = null!;
        public UserAccessInfoDto AccessInfo { get; set; } = null!;
        public List<string> AvailableModules { get; set; } = new List<string>();
    }
}
