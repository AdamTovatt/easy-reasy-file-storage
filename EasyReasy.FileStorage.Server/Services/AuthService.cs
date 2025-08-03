using EasyReasy.Auth;
using EasyReasy.FileStorage.Remote.Common;
using System.Security.Claims;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Custom authentication service that validates username/password and creates JWT tokens with user claims.
    /// This service handles the custom claims for user ID and tenant ID that our file storage system requires.
    /// </summary>
    public class AuthService : IAuthRequestValidationService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserServiceFactory _userServiceFactory;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IJwtTokenService jwtTokenService, IUserServiceFactory userServiceFactory, IPasswordHasher passwordHasher)
        {
            _jwtTokenService = jwtTokenService;
            _userServiceFactory = userServiceFactory;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Validates API key requests. Not supported in this implementation.
        /// </summary>
        public Task<AuthResponse?> ValidateApiKeyRequestAsync(ApiKeyAuthRequest request, IJwtTokenService jwtTokenService, HttpContext? httpContext = null)
        {
            // API key authentication is not supported in this implementation
            return Task.FromResult<AuthResponse?>(null);
        }

        /// <summary>
        /// Validates username/password requests and creates JWT tokens with user claims.
        /// Requires X-Tenant-ID header to be present.
        /// </summary>
        public async Task<AuthResponse?> ValidateLoginRequestAsync(LoginAuthRequest request, IJwtTokenService jwtTokenService, HttpContext? httpContext = null)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            // Extract tenant ID from request headers
            string? tenantId = null;
            if (httpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out Microsoft.Extensions.Primitives.StringValues headerTenantId) == true)
            {
                tenantId = headerTenantId.ToString();
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return null;
            }

            // Create tenant-specific user service
            IUserService userService = _userServiceFactory.CreateUserService(tenantId);

            // Retrieve user from the user service
            User? user = await userService.GetUserByNameAsync(request.Username);
            if (user == null)
            {
                return null;
            }

            // Validate password
            if (!_passwordHasher.ValidatePassword(request.Password, user.PasswordHash, user.Id))
            {
                return null;
            }

            // Determine user roles based on admin status
            string[] roles = user.IsAdmin ? new[] { "admin", "user" } : new[] { "user" };

            // Create JWT token with user claims including tenant ID
            DateTime expiresAt = DateTime.UtcNow.AddHours(1);
            string token = jwtTokenService.CreateToken(
                subject: user.Id,
                authType: "user",
                additionalClaims: new[]
                {
                    new Claim("user_id", user.Id),
                    new Claim("tenant_id", tenantId)
                },
                roles: roles,
                expiresAt: expiresAt);

            return new AuthResponse(token, expiresAt.ToString("o"));
        }
    }
}