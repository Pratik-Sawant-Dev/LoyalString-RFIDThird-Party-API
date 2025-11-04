using System.ComponentModel.DataAnnotations;

namespace RfidAppApi.DTOs
{
    /// <summary>
    /// User data transfer object for API responses
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username for login
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email address (unique)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the user
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Mobile phone number
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Fax number
        /// </summary>
        public string? FaxNumber { get; set; }

        /// <summary>
        /// City where the user is located
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Organization/company name
        /// </summary>
        public string OrganisationName { get; set; } = string.Empty;

        /// <summary>
        /// Type of showroom (e.g., Premium, Standard)
        /// </summary>
        public string? ShowroomType { get; set; }

        /// <summary>
        /// Unique client code (e.g., LS0001)
        /// </summary>
        public string ClientCode { get; set; } = string.Empty;

        /// <summary>
        /// Database name for the client
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Connection string for the client database
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Whether the user is an admin
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// User type (MainAdmin, Admin, User)
        /// </summary>
        public string UserType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the admin user who created this user (null for main admin)
        /// </summary>
        public int? AdminUserId { get; set; }

        /// <summary>
        /// Whether the user is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Last login date
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new user
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// Username for login (required)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email address (required, unique)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password (required)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the user
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Mobile phone number
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Fax number
        /// </summary>
        public string? FaxNumber { get; set; }

        /// <summary>
        /// City where the user is located
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Organization/company name (required)
        /// </summary>
        public string OrganisationName { get; set; } = string.Empty;

        /// <summary>
        /// Type of showroom (e.g., Premium, Standard)
        /// </summary>
        public string? ShowroomType { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating user information
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Full name of the user
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Mobile phone number
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Fax number
        /// </summary>
        public string? FaxNumber { get; set; }

        /// <summary>
        /// City where the user is located
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Organization/company name
        /// </summary>
        public string? OrganisationName { get; set; }

        /// <summary>
        /// Type of showroom (e.g., Premium, Standard)
        /// </summary>
        public string? ShowroomType { get; set; }

        /// <summary>
        /// Whether the user is active
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Data transfer object for user login
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email address (required)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password (required)
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for login response
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT authentication token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// User information
        /// </summary>
        public UserDto User { get; set; } = null!;

        /// <summary>
        /// When the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User permissions for all modules
        /// </summary>
        public List<UserPermissionDto> Permissions { get; set; } = new List<UserPermissionDto>();

        /// <summary>
        /// Permission summary for quick overview
        /// </summary>
        public UserPermissionSummaryDto PermissionSummary { get; set; } = null!;

        /// <summary>
        /// Branch and counter access information
        /// </summary>
        public UserAccessInfoDto AccessInfo { get; set; } = null!;
    }

    /// <summary>
    /// Data transfer object for user access information
    /// </summary>
    public class UserAccessInfoDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? CounterId { get; set; }
        public string? CounterName { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public List<int> AccessibleBranchIds { get; set; } = new List<int>();
        public List<int> AccessibleCounterIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Data transfer object for forgot password request
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// Email address of the user (required)
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for reset password request
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Password reset token received from forgot password email (required)
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user (required)
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// New password (required, minimum 6 characters)
        /// </summary>
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password (required, must match NewPassword)
        /// </summary>
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for forgot password response
    /// </summary>
    public class ForgotPasswordResponseDto
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Password reset token (for testing purposes - remove in production)
        /// </summary>
        public string? ResetToken { get; set; }

        /// <summary>
        /// Token expiry time
        /// </summary>
        public DateTime? TokenExpiry { get; set; }
    }

    /// <summary>
    /// Data transfer object for reset password response
    /// </summary>
    public class ResetPasswordResponseDto
    {
        /// <summary>
        /// Whether the password reset was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    #region User Profile DTOs

    /// <summary>
    /// Data transfer object for updating user profile
    /// </summary>
    public class UpdateUserProfileDto
    {
        /// <summary>
        /// Address line 1
        /// </summary>
        [StringLength(255)]
        public string? AddressLine1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
        [StringLength(255)]
        public string? AddressLine2 { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [StringLength(100)]
        public string? State { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [StringLength(100)]
        public string? Country { get; set; }

        /// <summary>
        /// Postal/Zip code
        /// </summary>
        [StringLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Landmark
        /// </summary>
        [StringLength(100)]
        public string? Landmark { get; set; }

        /// <summary>
        /// Bio/Description
        /// </summary>
        [StringLength(500)]
        public string? Bio { get; set; }

        /// <summary>
        /// Designation/Job title
        /// </summary>
        [StringLength(100)]
        public string? Designation { get; set; }

        /// <summary>
        /// Alternate phone number
        /// </summary>
        [StringLength(50)]
        public string? AlternatePhone { get; set; }
    }

    /// <summary>
    /// Data transfer object for user profile response
    /// </summary>
    public class UserProfileResponseDto
    {
        public int UserProfileId { get; set; }
        public int UserId { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? ProfileImageFileName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ProfileImageContentType { get; set; }
        public long? ProfileImageFileSize { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? Landmark { get; set; }
        public string? Bio { get; set; }
        public string? Designation { get; set; }
        public string? AlternatePhone { get; set; }
        public bool IsProfileComplete { get; set; }
        public DateTime? ProfileCompletedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        
        // User basic info
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
    }

    /// <summary>
    /// Data transfer object for complete user profile (including user data)
    /// </summary>
    public class CompleteUserProfileDto
    {
        public UserProfileResponseDto? Profile { get; set; }
        public UserDto? User { get; set; }
        public double ProfileCompletionPercentage { get; set; }
    }

    #endregion

} 