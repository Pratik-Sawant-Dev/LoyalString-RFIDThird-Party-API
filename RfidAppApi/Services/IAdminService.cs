using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IAdminService
    {
        // User Management
        Task<AdminUserDto> CreateUserAsync(CreateUserByAdminDto createUserDto, int adminUserId);
        Task<AdminUserDto> UpdateUserAsync(int userId, UpdateUserByAdminDto updateUserDto, int adminUserId);
        Task<bool> DeleteUserAsync(int userId, int adminUserId);
        Task<AdminUserDto?> GetUserByIdAsync(int userId, int adminUserId);
        Task<IEnumerable<AdminUserDto>> GetUsersByAdminAsync(int adminUserId);
        Task<IEnumerable<AdminUserDto>> GetAllUsersInOrganizationAsync(string clientCode);

        // Permission Management
        Task<IEnumerable<UserPermissionDto>> GetUserPermissionsAsync(int userId);
        Task<bool> UpdateUserPermissionsAsync(int userId, List<UserPermissionCreateDto> permissions, int adminUserId);
        Task<bool> BulkUpdatePermissionsAsync(BulkPermissionUpdateDto bulkUpdate, int adminUserId);
        Task<IEnumerable<UserPermissionDto>> GetAllUserPermissionsAsync(string clientCode);
        
        // Permission Removal
        Task<bool> RemoveUserPermissionsAsync(int userId, int adminUserId);
        Task<bool> RemoveUserPermissionAsync(int userId, string module, int adminUserId);
        Task<bool> BulkRemovePermissionsAsync(BulkPermissionRemoveDto bulkRemove, int adminUserId);
        
        // Permission Analytics
        Task<UserPermissionSummaryDto> GetUserPermissionSummaryAsync(int userId);

        // Activity Tracking
        Task LogActivityAsync(int userId, string clientCode, string activityType, string action, 
            string? description = null, string? tableName = null, int? recordId = null, 
            object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
        
        Task<IEnumerable<UserActivityDto>> GetUserActivitiesAsync(ActivityFilterDto filter, string clientCode);
        Task<IEnumerable<UserActivityDto>> GetAllActivitiesAsync(ActivityFilterDto filter, int adminUserId);

        // Activity Analytics
        Task<ActivitySummaryDto> GetActivitySummaryAsync(int adminUserId);
        Task<ActivitySummaryDto> GetActivitySummaryByDateRangeAsync(int adminUserId, DateTime startDate, DateTime endDate);
        Task<ActivitySummaryDto> GetActivitySummaryByUserAsync(int adminUserId, int userId);
        Task<ActivitySummaryDto> GetActivitySummaryByModuleAsync(int adminUserId, string module);

        // User Hierarchy
        Task<UserHierarchyDto> GetUserHierarchyAsync(int adminUserId);
        Task<UserHierarchyDto> GetUserHierarchyByAdminAsync(int adminUserId);

        // Data Export
        Task<string> ExportActivitiesToCsvAsync(ActivityFilterDto filter, int adminUserId);
        Task<string> ExportPermissionsToCsvAsync(string clientCode);

        // Dashboard and Analytics
        Task<AdminDashboardDto> GetAdminDashboardAsync(int adminUserId);
        Task<AdminDashboardDto> GetOrganizationDashboardAsync(string clientCode);

        // User Status Management
        Task<bool> ActivateUserAsync(int userId, int adminUserId);
        Task<bool> DeactivateUserAsync(int userId, int adminUserId);
        Task<bool> ResetUserPasswordAsync(int userId, string newPassword, int adminUserId);

        // Validation
        Task<bool> CanUserAccessUserAsync(int adminUserId, int targetUserId);
        Task<bool> HasPermissionAsync(int userId, string module, string action);

        // Branch and Counter Management
        Task<IEnumerable<BranchMasterDto>> GetBranchesAsync(string clientCode);
        Task<IEnumerable<CounterMasterDto>> GetCountersByBranchAsync(int branchId, string clientCode);
        Task<IEnumerable<AdminUserDto>> GetUsersByLocationAsync(int adminUserId, int? branchId = null, int? counterId = null);
    }
}
