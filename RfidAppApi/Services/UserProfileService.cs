using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for managing user profiles including profile images and extended address information
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserProfileService> _logger;
        
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB for profile images
        private readonly int _maxImageDimension = 2048; // 2K resolution for profile images

        public UserProfileService(
            AppDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<UserProfileService> logger)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task<UserProfileResponseDto> UploadProfileImageAsync(int userId, IFormFile file)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Validate file
            if (!await ValidateImageFileAsync(file))
            {
                throw new ArgumentException("Invalid image file. Allowed formats: JPG, JPEG, PNG, GIF, BMP, WEBP. Max size: 5MB.");
            }

            // Get or create user profile
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
            }

            // Delete old profile image if exists
            if (!string.IsNullOrEmpty(profile.ProfileImagePath) && File.Exists(profile.ProfileImagePath))
            {
                try
                {
                    File.Delete(profile.ProfileImagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old profile image for user {UserId}", userId);
                }
            }

            // Create upload directory
            var uploadPath = GetUploadPath();
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Resize image if needed (optional - for profile images, we can keep original but optimize)
            var optimizedPath = await OptimizeProfileImageAsync(filePath, uploadPath);

            // Update profile
            profile.ProfileImagePath = optimizedPath ?? filePath;
            profile.ProfileImageFileName = fileName;
            profile.ProfileImageContentType = file.ContentType;
            profile.ProfileImageFileSize = file.Length;
            profile.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Profile image uploaded for user {UserId}", userId);

            return await MapToResponseDtoAsync(profile);
        }

        public async Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Get or create user profile
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
            }

            // Update profile fields
            if (updateDto.AddressLine1 != null)
                profile.AddressLine1 = updateDto.AddressLine1;
            if (updateDto.AddressLine2 != null)
                profile.AddressLine2 = updateDto.AddressLine2;
            if (updateDto.City != null)
            {
                // Update user's city if provided
                user.City = updateDto.City;
            }
            if (updateDto.State != null)
                profile.State = updateDto.State;
            if (updateDto.Country != null)
                profile.Country = updateDto.Country;
            if (updateDto.PostalCode != null)
                profile.PostalCode = updateDto.PostalCode;
            if (updateDto.Landmark != null)
                profile.Landmark = updateDto.Landmark;
            if (updateDto.Bio != null)
                profile.Bio = updateDto.Bio;
            if (updateDto.Designation != null)
                profile.Designation = updateDto.Designation;
            if (updateDto.AlternatePhone != null)
                profile.AlternatePhone = updateDto.AlternatePhone;

            // Update user's address field if AddressLine1 is provided
            if (updateDto.AddressLine1 != null)
            {
                user.Address = updateDto.AddressLine1;
                if (!string.IsNullOrEmpty(updateDto.AddressLine2))
                {
                    user.Address += ", " + updateDto.AddressLine2;
                }
            }

            // Check profile completion
            var completionPercentage = await CalculateProfileCompletionAsync(userId);
            profile.IsProfileComplete = completionPercentage >= 80; // 80% threshold
            if (profile.IsProfileComplete && profile.ProfileCompletedOn == null)
            {
                profile.ProfileCompletedOn = DateTime.UtcNow;
            }

            profile.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Profile updated for user {UserId}", userId);

            return await MapToResponseDtoAsync(profile);
        }

        public async Task<UserProfileResponseDto?> GetProfileAsync(int userId)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return null;

            return await MapToResponseDtoAsync(profile);
        }

        public async Task<CompleteUserProfileDto> GetCompleteProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.AdminUser)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var profileDto = profile != null ? await MapToResponseDtoAsync(profile) : null;
            var completionPercentage = await CalculateProfileCompletionAsync(userId);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                MobileNumber = user.MobileNumber,
                FaxNumber = user.FaxNumber,
                City = user.City,
                Address = user.Address,
                OrganisationName = user.OrganisationName,
                ShowroomType = user.ShowroomType,
                ClientCode = user.ClientCode,
                DatabaseName = user.DatabaseName,
                IsAdmin = user.IsAdmin,
                UserType = user.UserType,
                AdminUserId = user.AdminUserId,
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn,
                LastLoginDate = user.LastLoginDate
            };

            return new CompleteUserProfileDto
            {
                Profile = profileDto,
                User = userDto,
                ProfileCompletionPercentage = completionPercentage
            };
        }

        public async Task<bool> DeleteProfileImageAsync(int userId)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null || string.IsNullOrEmpty(profile.ProfileImagePath))
            {
                return false;
            }

            // Delete physical file
            try
            {
                if (File.Exists(profile.ProfileImagePath))
                {
                    File.Delete(profile.ProfileImagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete profile image file for user {UserId}", userId);
                throw new InvalidOperationException("Failed to delete profile image file.", ex);
            }

            // Clear profile image fields
            profile.ProfileImagePath = null;
            profile.ProfileImageFileName = null;
            profile.ProfileImageContentType = null;
            profile.ProfileImageFileSize = null;
            profile.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Profile image deleted for user {UserId}", userId);

            return true;
        }

        public async Task<double> CalculateProfileCompletionAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return 0;

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            int totalFields = 12; // Total number of profile fields
            int filledFields = 0;

            // User basic fields
            if (!string.IsNullOrEmpty(user.FullName))
                filledFields++;
            if (!string.IsNullOrEmpty(user.Email))
                filledFields++;
            if (!string.IsNullOrEmpty(user.MobileNumber))
                filledFields++;

            // Profile fields
            if (profile != null)
            {
                if (!string.IsNullOrEmpty(profile.ProfileImagePath))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.AddressLine1))
                    filledFields++;
                if (!string.IsNullOrEmpty(user.City))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.State))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.Country))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.PostalCode))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.Designation))
                    filledFields++;
                if (!string.IsNullOrEmpty(profile.Bio))
                    filledFields++;
            }

            return Math.Round((double)filledFields / totalFields * 100, 2);
        }

        #region Private Helper Methods

        private async Task<UserProfileResponseDto> MapToResponseDtoAsync(UserProfile profile)
        {
            var user = await _context.Users.FindAsync(profile.UserId);

            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7107";
            var imageUrl = !string.IsNullOrEmpty(profile.ProfileImagePath)
                ? $"{baseUrl}/api/UserProfile/image/{profile.UserId}"
                : null;

            return new UserProfileResponseDto
            {
                UserProfileId = profile.UserProfileId,
                UserId = profile.UserId,
                ProfileImagePath = profile.ProfileImagePath,
                ProfileImageFileName = profile.ProfileImageFileName,
                ProfileImageUrl = imageUrl,
                ProfileImageContentType = profile.ProfileImageContentType,
                ProfileImageFileSize = profile.ProfileImageFileSize,
                AddressLine1 = profile.AddressLine1,
                AddressLine2 = profile.AddressLine2,
                City = user?.City,
                State = profile.State,
                Country = profile.Country,
                PostalCode = profile.PostalCode,
                Landmark = profile.Landmark,
                Bio = profile.Bio,
                Designation = profile.Designation,
                AlternatePhone = profile.AlternatePhone,
                IsProfileComplete = profile.IsProfileComplete,
                ProfileCompletedOn = profile.ProfileCompletedOn,
                CreatedOn = profile.CreatedOn,
                UpdatedOn = profile.UpdatedOn,
                FullName = user?.FullName,
                Email = user?.Email,
                MobileNumber = user?.MobileNumber
            };
        }

        private string GetUploadPath()
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "profiles");
            return uploadsPath;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var uniqueName = $"{Guid.NewGuid()}{extension}";
            return uniqueName;
        }

        private async Task<bool> ValidateImageFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Validate image format by trying to load it
            try
            {
                using var image = Image.FromStream(file.OpenReadStream());
                if (image.Width > _maxImageDimension || image.Height > _maxImageDimension)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task<string?> OptimizeProfileImageAsync(string filePath, string uploadPath)
        {
            try
            {
                using var image = Image.FromStream(File.OpenRead(filePath));
                
                // For profile images, we can create a square version (optional)
                // For now, we'll just return the original path
                // You can add image optimization logic here if needed
                
                return null; // Return null to keep original
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}

