using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Extensions;
using RfidAppApi.Services;
using System.Security.Claims;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// Controller for managing user profiles including profile images and extended address information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserProfileService userProfileService,
            ILogger<UserProfileController> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        /// <summary>
        /// Upload profile image
        /// </summary>
        /// <param name="file">Profile image file</param>
        /// <returns>User profile with image</returns>
        [HttpPost("upload-image")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserProfileResponseDto>> UploadProfileImage([FromForm] IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file provided." });
                }

                var result = await _userProfileService.UploadProfileImageAsync(userId, file);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while uploading the profile image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user profile information (address, bio, designation, etc.)
        /// </summary>
        /// <param name="updateDto">Profile update data</param>
        /// <returns>Updated user profile</returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserProfileResponseDto>> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var result = await _userProfileService.UpdateProfileAsync(userId, updateDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while updating the profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        /// <returns>User profile</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserProfileResponseDto>> GetMyProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _userProfileService.GetProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { message = "Profile not found. Please complete your profile." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while retrieving the profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get complete user profile including user data and completion percentage
        /// </summary>
        /// <returns>Complete user profile</returns>
        [HttpGet("complete")]
        [ProducesResponseType(typeof(CompleteUserProfileDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CompleteUserProfileDto>> GetCompleteProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userProfileService.GetCompleteProfileAsync(userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complete profile for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while retrieving the complete profile.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete profile image
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete("image")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteProfileImage()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userProfileService.DeleteProfileImageAsync(userId);

                if (!result)
                {
                    return NotFound(new { message = "Profile image not found." });
                }

                return Ok(new { message = "Profile image deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile image for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while deleting the profile image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile completion percentage
        /// </summary>
        /// <returns>Profile completion percentage</returns>
        [HttpGet("completion")]
        [ProducesResponseType(typeof(double), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<double>> GetProfileCompletion()
        {
            try
            {
                var userId = GetCurrentUserId();
                var percentage = await _userProfileService.CalculateProfileCompletionAsync(userId);
                return Ok(new { completionPercentage = percentage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating profile completion for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while calculating profile completion.", error = ex.Message });
            }
        }

        /// <summary>
        /// Serve profile image
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Image file</returns>
        [HttpGet("image/{userId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileImage(int userId)
        {
            try
            {
                var profile = await _userProfileService.GetProfileAsync(userId);
                
                if (profile == null || string.IsNullOrEmpty(profile.ProfileImagePath))
                {
                    return NotFound(new { message = "Profile image not found." });
                }

                if (!System.IO.File.Exists(profile.ProfileImagePath))
                {
                    return NotFound(new { message = "Profile image file not found." });
                }

                var imageBytes = await System.IO.File.ReadAllBytesAsync(profile.ProfileImagePath);
                var contentType = profile.ProfileImageContentType ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving profile image for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while serving the profile image.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get profile by user ID (Admin only - for managing sub-users)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User profile</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<UserProfileResponseDto>> GetProfileByUserId(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                // Check if user is accessing their own profile or is an admin
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only access your own profile unless you are an admin.");
                }

                var profile = await _userProfileService.GetProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { message = "Profile not found." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving the profile.", error = ex.Message });
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return userId;
        }

        #endregion
    }
}

