using EasyReasy.Auth;
using EasyReasy.FileStorage.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyReasy.FileStorage.Server.Controllers
{
    /// <summary>
    /// Admin controller for user management operations.
    /// Requires admin role and operates within the admin's tenant.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserServiceFactory _userServiceFactory;

        public AdminController(IUserServiceFactory userServiceFactory)
        {
            _userServiceFactory = userServiceFactory;
        }

        /// <summary>
        /// Creates a new user in the admin's tenant.
        /// </summary>
        /// <param name="request">The user creation request.</param>
        /// <returns>Created user information or error details.</returns>
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract tenant ID from JWT claims
            string? tenantId = User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest("Tenant ID not found in token claims");
            }

            // Create tenant-specific user service
            IUserService userService = _userServiceFactory.CreateUserService(tenantId);

            // Check if user already exists
            var existingUser = await userService.GetUserByNameAsync(request.Username);
            if (existingUser != null)
            {
                return Conflict($"User '{request.Username}' already exists in tenant '{tenantId}'");
            }

            // Create the user
            bool success = await userService.CreateUserAsync(request.Username, request.Password);
            if (!success)
            {
                return BadRequest("Failed to create user");
            }

            return CreatedAtAction(nameof(GetUser), new { username = request.Username }, new
            {
                Username = request.Username,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Gets user information from the admin's tenant.
        /// </summary>
        /// <param name="username">The username to retrieve.</param>
        /// <returns>User information or 404 if not found.</returns>
        [HttpGet("users/{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            // Extract tenant ID from JWT claims
            string? tenantId = User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest("Tenant ID not found in token claims");
            }

            // Create tenant-specific user service
            IUserService userService = _userServiceFactory.CreateUserService(tenantId);

            // Get user information
            var user = await userService.GetUserByNameAsync(username);
            if (user == null)
            {
                return NotFound($"User '{username}' not found in tenant '{tenantId}'");
            }

            return Ok(new
            {
                Username = user.Id,
                TenantId = tenantId,
                HasPassword = !string.IsNullOrEmpty(user.PasswordHash)
            });
        }
    }

    /// <summary>
    /// Request model for creating a new user.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// The username for the new user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password for the new user.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
} 