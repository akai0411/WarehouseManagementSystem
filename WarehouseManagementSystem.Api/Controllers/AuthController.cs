using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Auth;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Handles authentication operations such as user login and token generation.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the AuthController.
        /// </summary>
        /// <param name="tokenService">Service responsible for generating JWT tokens.</param>
        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="request">Login credentials (email and password).</param>
        /// <returns>JWT token if authentication is successful.</returns>
        /// <response code="200">Returns the JWT token.</response>
        /// <response code="401">If credentials are invalid.</response>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Email != "admin@test.com" || request.Password != "1234")
                return Unauthorized();

            var token = _tokenService.CreateToken(request.Email);

            return Ok(new AuthResponse
            {
                Token = token
            });
        }
    }
}