using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public UserService(
            AppDbContext context, 
            IClientDatabaseService clientDatabaseService,
            IConfiguration configuration)
        {
            _context = context;
            _clientDatabaseService = clientDatabaseService;
            _configuration = configuration;
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
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
                CreatedOn = user.CreatedOn
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
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn
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
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn
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
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn
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
            return user.PasswordHash == hashedPassword;
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
                    new Claim("OrganisationName", user.OrganisationName)
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
    }
} 