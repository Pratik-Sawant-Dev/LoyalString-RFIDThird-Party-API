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
    }
} 