using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Interface for user profile service
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Upload profile image for a user
        /// </summary>
        Task<UserProfileResponseDto> UploadProfileImageAsync(int userId, IFormFile file);

        /// <summary>
        /// Update user profile information
        /// </summary>
        Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateUserProfileDto updateDto);

        /// <summary>
        /// Get user profile by user ID
        /// </summary>
        Task<UserProfileResponseDto?> GetProfileAsync(int userId);

        /// <summary>
        /// Get complete user profile with user data
        /// </summary>
        Task<CompleteUserProfileDto> GetCompleteProfileAsync(int userId);

        /// <summary>
        /// Delete profile image
        /// </summary>
        Task<bool> DeleteProfileImageAsync(int userId);

        /// <summary>
        /// Calculate profile completion percentage
        /// </summary>
        Task<double> CalculateProfileCompletionAsync(int userId);
    }
}

