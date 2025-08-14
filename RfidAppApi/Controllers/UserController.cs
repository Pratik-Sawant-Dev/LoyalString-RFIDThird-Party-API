using Microsoft.AspNetCore.Mvc;
using RfidAppApi.DTOs;
using RfidAppApi.Services;

namespace RfidAppApi.Controllers
{
    /// <summary>
    /// User management controller for handling user registration, authentication, and management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Register a new client organization with automatic database creation
        /// </summary>
        /// <param name="createUserDto">User registration details</param>
        /// <returns>Created user information with client code and database details</returns>
        /// <response code="201">User successfully registered</response>
        /// <response code="400">Invalid input or email already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> Register([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.RegisterUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
            }
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isValid = await _userService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password);
                if (!isValid)
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                var user = await _userService.GetUserByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found." });
                }

                var token = await _userService.GenerateJwtTokenAsync(user);
                var response = new LoginResponseDto
                {
                    Token = token,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        /// <response code="200">User found</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User information</returns>
        /// <response code="200">User found</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by client code
        /// </summary>
        /// <param name="clientCode">Client code (e.g., LS0001)</param>
        /// <returns>User information</returns>
        /// <response code="200">User found</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("by-client-code/{clientCode}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> GetUserByClientCode(string clientCode)
        {
            try
            {
                var user = await _userService.GetUserByClientCodeAsync(clientCode);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of all active users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">Updated user information</param>
        /// <returns>Updated user information</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (soft delete - sets IsActive to false)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        /// <response code="204">User deleted successfully</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "User not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the user.", error = ex.Message });
            }
        }
    }
} 