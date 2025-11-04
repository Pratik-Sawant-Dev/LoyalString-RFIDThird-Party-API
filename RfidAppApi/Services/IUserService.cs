using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> GetUserByClientCodeAsync(string clientCode);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<string> GenerateJwtTokenAsync(UserDto user);
        Task<LoginResponseDto> GenerateLoginResponseAsync(UserDto user);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
} 