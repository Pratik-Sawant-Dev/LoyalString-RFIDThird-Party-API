using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for user permission management and checking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserPermissionController : ControllerBase
    {
        private readonly IUserPermissionService _userPermissionService;

        public UserPermissionController(IUserPermissionService userPermissionService)
        {
            _userPermissionService = userPermissionService;
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        /// <returns>List of user permissions</returns>
        /// <response code="200">Permissions retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-permissions")]
        [ProducesResponseType(typeof(IEnumerable<UserPermissionDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserPermissionDto>>> GetMyPermissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var permissions = await _userPermissionService.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving permissions.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's permission summary
        /// </summary>
        /// <returns>User permission summary</returns>
        /// <response code="200">Permission summary retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-permission-summary")]
        [ProducesResponseType(typeof(UserPermissionSummaryDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserPermissionSummaryDto>> GetMyPermissionSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                var summary = await _userPermissionService.GetUserPermissionSummaryAsync(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving permission summary.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's access information
        /// </summary>
        /// <returns>User access information</returns>
        /// <response code="200">Access information retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-access-info")]
        [ProducesResponseType(typeof(UserAccessInfoDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserAccessInfoDto>> GetMyAccessInfo()
        {
            try
            {
                var userId = GetCurrentUserId();
                var accessInfo = await _userPermissionService.GetUserAccessInfoAsync(userId);
                return Ok(accessInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving access information.", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if current user has specific permission
        /// </summary>
        /// <param name="module">Module name</param>
        /// <param name="action">Action (view, create, edit, delete, export, import)</param>
        /// <returns>Permission check result</returns>
        /// <response code="200">Permission check completed</response>
        /// <response code="400">Invalid parameters</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("check-permission")]
        [ProducesResponseType(typeof(PermissionCheckResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PermissionCheckResult>> CheckPermission(
            [FromQuery] string module, 
            [FromQuery] string action)
        {
            try
            {
                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(action))
                {
                    return BadRequest(new { message = "Module and action parameters are required." });
                }

                var userId = GetCurrentUserId();
                var hasPermission = await _userPermissionService.HasPermissionAsync(userId, module, action);

                var result = new PermissionCheckResult
                {
                    UserId = userId,
                    Module = module,
                    Action = action,
                    HasPermission = hasPermission,
                    CheckedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking permission.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all available permission modules
        /// </summary>
        /// <returns>List of available modules</returns>
        /// <response code="200">Modules retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("available-modules")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableModules()
        {
            try
            {
                var modules = await _userPermissionService.GetAvailableModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving modules.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's accessible branch IDs
        /// </summary>
        /// <returns>List of accessible branch IDs</returns>
        /// <response code="200">Branch IDs retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-accessible-branches")]
        [ProducesResponseType(typeof(IEnumerable<int>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<int>>> GetMyAccessibleBranches()
        {
            try
            {
                var userId = GetCurrentUserId();
                var branchIds = await _userPermissionService.GetAccessibleBranchIdsAsync(userId);
                return Ok(branchIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving accessible branches.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's accessible counter IDs
        /// </summary>
        /// <returns>List of accessible counter IDs</returns>
        /// <response code="200">Counter IDs retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-accessible-counters")]
        [ProducesResponseType(typeof(IEnumerable<int>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<int>>> GetMyAccessibleCounters()
        {
            try
            {
                var userId = GetCurrentUserId();
                var counterIds = await _userPermissionService.GetAccessibleCounterIdsAsync(userId);
                return Ok(counterIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving accessible counters.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get comprehensive permission details for current user
        /// </summary>
        /// <returns>Complete user permission details</returns>
        /// <response code="200">Permission details retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-permission-details")]
        [ProducesResponseType(typeof(UserPermissionDetailsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserPermissionDetailsDto>> GetMyPermissionDetails()
        {
            try
            {
                var userId = GetCurrentUserId();
                var details = await _userPermissionService.GetUserPermissionDetailsAsync(userId);
                return Ok(details);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving permission details.", error = ex.Message });
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        #endregion
    }

    /// <summary>
    /// Data transfer object for permission check result
    /// </summary>
    public class PermissionCheckResult
    {
        public int UserId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool HasPermission { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}
