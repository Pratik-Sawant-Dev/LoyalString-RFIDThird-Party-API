using RfidAppApi.Models;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing user access control based on branch and counter assignments
    /// </summary>
    public interface IAccessControlService
    {
        /// <summary>
        /// Get user's assigned branch and counter information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's branch and counter assignment, or null if user not found</returns>
        Task<UserAccessInfo?> GetUserAccessInfoAsync(int userId);

        /// <summary>
        /// Check if user can access a specific branch
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="branchId">Branch ID to check</param>
        /// <returns>True if user can access the branch, false otherwise</returns>
        Task<bool> CanAccessBranchAsync(int userId, int branchId);

        /// <summary>
        /// Check if user can access a specific counter
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="counterId">Counter ID to check</param>
        /// <returns>True if user can access the counter, false otherwise</returns>
        Task<bool> CanAccessCounterAsync(int userId, int counterId);

        /// <summary>
        /// Check if user can access a specific branch and counter combination
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="branchId">Branch ID to check</param>
        /// <param name="counterId">Counter ID to check</param>
        /// <returns>True if user can access the branch and counter, false otherwise</returns>
        Task<bool> CanAccessBranchAndCounterAsync(int userId, int branchId, int counterId);

        /// <summary>
        /// Check if user is an admin (can access all branches and counters)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user is admin, false otherwise</returns>
        Task<bool> IsAdminUserAsync(int userId);

        /// <summary>
        /// Get all branch IDs that the user can access
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of accessible branch IDs</returns>
        Task<List<int>> GetAccessibleBranchIdsAsync(int userId);

        /// <summary>
        /// Get all counter IDs that the user can access
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of accessible counter IDs</returns>
        Task<List<int>> GetAccessibleCounterIdsAsync(int userId);
    }

    /// <summary>
    /// Information about user's branch and counter access
    /// </summary>
    public class UserAccessInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CounterId { get; set; }
        public string? CounterName { get; set; }
        public string ClientCode { get; set; } = string.Empty;
    }
}
