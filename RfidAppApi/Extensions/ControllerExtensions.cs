using Microsoft.AspNetCore.Mvc;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Extensions
{
    /// <summary>
    /// Extension methods for controllers to handle access control
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <returns>User ID or 0 if not found</returns>
        public static int GetCurrentUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// Get client code from JWT token
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <returns>Client code or empty string if not found</returns>
        public static string GetClientCodeFromToken(this ControllerBase controller)
        {
            return controller.User.FindFirst("ClientCode")?.Value ?? string.Empty;
        }

        /// <summary>
        /// Check if current user can access a specific branch
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="accessControlService">Access control service</param>
        /// <param name="branchId">Branch ID to check</param>
        /// <returns>True if user can access the branch</returns>
        public static async Task<bool> CanAccessBranchAsync(this ControllerBase controller, IAccessControlService accessControlService, int branchId)
        {
            var userId = controller.GetCurrentUserId();
            if (userId == 0) return false;

            return await accessControlService.CanAccessBranchAsync(userId, branchId);
        }

        /// <summary>
        /// Check if current user can access a specific counter
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="accessControlService">Access control service</param>
        /// <param name="counterId">Counter ID to check</param>
        /// <returns>True if user can access the counter</returns>
        public static async Task<bool> CanAccessCounterAsync(this ControllerBase controller, IAccessControlService accessControlService, int counterId)
        {
            var userId = controller.GetCurrentUserId();
            if (userId == 0) return false;

            return await accessControlService.CanAccessCounterAsync(userId, counterId);
        }

        /// <summary>
        /// Check if current user can access a specific branch and counter combination
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="accessControlService">Access control service</param>
        /// <param name="branchId">Branch ID to check</param>
        /// <param name="counterId">Counter ID to check</param>
        /// <returns>True if user can access the branch and counter</returns>
        public static async Task<bool> CanAccessBranchAndCounterAsync(this ControllerBase controller, IAccessControlService accessControlService, int branchId, int counterId)
        {
            var userId = controller.GetCurrentUserId();
            if (userId == 0) return false;

            return await accessControlService.CanAccessBranchAndCounterAsync(userId, branchId, counterId);
        }

        /// <summary>
        /// Check if current user is an admin
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="accessControlService">Access control service</param>
        /// <returns>True if user is admin</returns>
        public static async Task<bool> IsAdminUserAsync(this ControllerBase controller, IAccessControlService accessControlService)
        {
            var userId = controller.GetCurrentUserId();
            if (userId == 0) return false;

            return await accessControlService.IsAdminUserAsync(userId);
        }

        /// <summary>
        /// Get current user's access information
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="accessControlService">Access control service</param>
        /// <returns>User access information or null if not found</returns>
        public static async Task<UserAccessInfo?> GetUserAccessInfoAsync(this ControllerBase controller, IAccessControlService accessControlService)
        {
            var userId = controller.GetCurrentUserId();
            if (userId == 0) return null;

            return await accessControlService.GetUserAccessInfoAsync(userId);
        }

        /// <summary>
        /// Create a forbidden response for access denied
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="message">Optional custom message</param>
        /// <returns>Forbidden result</returns>
        public static ActionResult AccessDenied(this ControllerBase controller, string? message = null)
        {
            return controller.Forbid(message ?? "Access denied. You don't have permission to access this resource.");
        }

        /// <summary>
        /// Create a forbidden response for branch access denied
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="branchId">Branch ID that was denied</param>
        /// <returns>Forbidden result</returns>
        public static ActionResult BranchAccessDenied(this ControllerBase controller, int branchId)
        {
            return controller.Forbid($"Access denied. You don't have permission to access branch ID {branchId}.");
        }

        /// <summary>
        /// Create a forbidden response for counter access denied
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="counterId">Counter ID that was denied</param>
        /// <returns>Forbidden result</returns>
        public static ActionResult CounterAccessDenied(this ControllerBase controller, int counterId)
        {
            return controller.Forbid($"Access denied. You don't have permission to access counter ID {counterId}.");
        }
    }
}
