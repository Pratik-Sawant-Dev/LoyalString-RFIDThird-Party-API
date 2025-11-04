using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RfidAppApi.Data;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RfidAppApi.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IClientDatabaseService _clientDatabaseService;
        private readonly IConfiguration _configuration;
        private readonly IUserPermissionService _userPermissionService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context, 
            IClientDatabaseService clientDatabaseService,
            IConfiguration configuration,
            IUserPermissionService userPermissionService,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _context = context;
            _clientDatabaseService = clientDatabaseService;
            _configuration = configuration;
            _userPermissionService = userPermissionService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                throw new InvalidOperationException("Email already registered.");
            }

            // Generate client code automatically (LS0001, LS0002, etc.)
            var clientCode = await _clientDatabaseService.GenerateClientCodeAsync();

            // Create client database
            var databaseName = await _clientDatabaseService.CreateClientDatabaseAsync(
                createUserDto.OrganisationName, 
                clientCode);

            // Hash password
            var passwordHash = HashPassword(createUserDto.Password);

            // Every user who registers through main registration API is a MainAdmin
            // because they are creating their own organization and database
            var isMainAdmin = true;

            // Create user
            var user = new User
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                PasswordHash = passwordHash,
                FullName = createUserDto.FullName,
                MobileNumber = createUserDto.MobileNumber,
                FaxNumber = createUserDto.FaxNumber,
                City = createUserDto.City,
                Address = createUserDto.Address,
                OrganisationName = createUserDto.OrganisationName,
                ShowroomType = createUserDto.ShowroomType,
                ClientCode = clientCode, // Auto-generated
                DatabaseName = databaseName,
                IsAdmin = isMainAdmin, // Every main registration user is admin
                UserType = "MainAdmin", // Every main registration user is MainAdmin
                AdminUserId = null, // Main admin has no parent admin
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send welcome email asynchronously (don't block registration if email fails)
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Attempting to send welcome email to {Email}", user.Email);
                    var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7107";
                    var loginUrl = $"{baseUrl}/login";
                    var emailSent = await _emailService.SendWelcomeEmailAsync(
                        user.Email,
                        user.UserName,
                        user.FullName ?? user.UserName,
                        user.OrganisationName,
                        loginUrl
                    );
                    
                    if (emailSent)
                    {
                        _logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send welcome email to {Email}. Check SMTP configuration.", user.Email);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't throw - email sending should not block registration
                    _logger.LogError(ex, "Error sending welcome email to {Email}: {ErrorMessage}", user.Email, ex.Message);
                }
            });

            // Return user DTO
            return new UserDto
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
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn,
                IsAdmin = user.IsAdmin,
                UserType = user.UserType
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            return new UserDto
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
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;

            return new UserDto
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
        }

        public async Task<UserDto?> GetUserByClientCodeAsync(string clientCode)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ClientCode == clientCode);

            if (user == null) return null;

            return new UserDto
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
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Update properties
            if (!string.IsNullOrEmpty(updateUserDto.FullName))
                user.FullName = updateUserDto.FullName;
            
            if (!string.IsNullOrEmpty(updateUserDto.MobileNumber))
                user.MobileNumber = updateUserDto.MobileNumber;
            
            if (!string.IsNullOrEmpty(updateUserDto.FaxNumber))
                user.FaxNumber = updateUserDto.FaxNumber;
            
            if (!string.IsNullOrEmpty(updateUserDto.City))
                user.City = updateUserDto.City;
            
            if (!string.IsNullOrEmpty(updateUserDto.Address))
                user.Address = updateUserDto.Address;
            
            if (!string.IsNullOrEmpty(updateUserDto.OrganisationName))
                user.OrganisationName = updateUserDto.OrganisationName;
            
            if (!string.IsNullOrEmpty(updateUserDto.ShowroomType))
                user.ShowroomType = updateUserDto.ShowroomType;
            
            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId) ?? throw new InvalidOperationException("Failed to retrieve updated user.");
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                MobileNumber = u.MobileNumber,
                FaxNumber = u.FaxNumber,
                City = u.City,
                Address = u.Address,
                OrganisationName = u.OrganisationName,
                ShowroomType = u.ShowroomType,
                ClientCode = u.ClientCode,
                DatabaseName = u.DatabaseName,
                IsActive = u.IsActive,
                CreatedOn = u.CreatedOn
            });
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null) return false;

            var hashedPassword = HashPassword(password);
            var isValid = user.PasswordHash == hashedPassword;

            if (isValid)
            {
                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log login activity
                await LogLoginActivityAsync(user.UserId, user.ClientCode);
            }

            return isValid;
        }

        private async Task LogLoginActivityAsync(int userId, string clientCode)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ClientCode = clientCode,
                ActivityType = "Authentication",
                Action = "Login",
                Description = "User logged in",
                CreatedOn = DateTime.UtcNow
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<LoginResponseDto> GenerateLoginResponseAsync(UserDto user)
        {
            var token = await GenerateJwtTokenAsync(user);
            var permissionDetails = await _userPermissionService.GetUserPermissionDetailsAsync(user.UserId);

            return new LoginResponseDto
            {
                Token = token,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Permissions = permissionDetails.Permissions.ToList(),
                PermissionSummary = permissionDetails.PermissionSummary,
                AccessInfo = permissionDetails.AccessInfo
            };
        }

        public Task<string> GenerateJwtTokenAsync(UserDto user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "YourSecretKeyHere");
            var issuer = jwtSettings["Issuer"] ?? "RfidAppApi";
            var audience = jwtSettings["Audience"] ?? "RfidAppApi";

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("ClientCode", user.ClientCode),
                    new Claim("OrganisationName", user.OrganisationName),
                    new Claim("IsAdmin", user.IsAdmin.ToString()),
                    new Claim("UserType", user.UserType),
                    new Claim("AdminUserId", user.AdminUserId?.ToString() ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && u.IsActive);
            
            if (user == null)
            {
                // Return success message even if user doesn't exist (security best practice)
                return new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "If an account with this email exists, a password reset link has been sent to your email."
                };
            }

            // Generate password reset token
            var token = GeneratePasswordResetToken();
            var tokenExpiry = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

            // Save token to user
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = tokenExpiry;
            await _context.SaveChangesAsync();

            // Log activity
            await LogLoginActivityAsync(user.UserId, user.ClientCode);

            // In production, send email with reset link
            // For now, return token in response (remove in production)
            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Password reset token has been generated. Please check your email for reset instructions.",
                ResetToken = token, // Remove this in production - only for testing
                TokenExpiry = tokenExpiry
            };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            // Validate passwords match
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "New password and confirm password do not match."
                };
            }

            // Validate password length
            if (resetPasswordDto.NewPassword.Length < 6)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Password must be at least 6 characters long."
                };
            }

            // Find user by email and token
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == resetPasswordDto.Email && 
                u.PasswordResetToken == resetPasswordDto.Token &&
                u.IsActive);

            if (user == null)
            {
                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token. Please request a new password reset."
                };
            }

            // Check if token has expired
            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                // Clear expired token
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _context.SaveChangesAsync();

                return new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Password reset token has expired. Please request a new password reset."
                };
            }

            // Update password
            user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            // Log activity
            await LogLoginActivityAsync(user.UserId, user.ClientCode);

            return new ResetPasswordResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully. You can now login with your new password."
            };
        }

        private string GeneratePasswordResetToken()
        {
            // Generate a secure random token (32 bytes = 44 base64 chars, we'll use 43 for URL safety)
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                var token = Convert.ToBase64String(bytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");
                
                // Ensure we have at least 32 characters (base64 encoding of 32 bytes gives ~43 chars)
                return token.Length >= 32 ? token.Substring(0, 32) : token;
            }
        }
    }
} 