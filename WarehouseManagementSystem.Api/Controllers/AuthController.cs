using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Auth.AssignRole;
using WarehouseManagementSystem.Application.Features.Auth.Login;
using WarehouseManagementSystem.Application.Features.Auth.Register;

namespace WarehouseManagementSystem.Api.Controllers
{
    /// <summary>
    /// Handles user authentication operations.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterHandler _registerHandler;
        private readonly LoginHandler _loginHandler;
        private readonly AssignRoleHandler _assignRoleHandler;
        /// <summary>
        /// Initializes a new instance of the AuthController.
        /// </summary>
        public AuthController(
            RegisterHandler registerHandler,
            LoginHandler loginHandler,
            AssignRoleHandler assignRoleHandler)
        {
            _registerHandler = registerHandler;
            _loginHandler = loginHandler;
            _assignRoleHandler = assignRoleHandler;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var userId = await _registerHandler.Handle(command);

            return Created(string.Empty,
                ApiResponse<Guid>.Ok(userId, "User registered successfully."));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var response = await _loginHandler.Handle(command);

            return Ok(ApiResponse<LoginResponse>.Ok(response));
        }
        /// <summary>
        /// Assigns a role and scope to a user. Admin and WarehouseManager only.
        /// </summary>
        [Authorize(Roles = "Admin,WarehouseManager")]
        [HttpPut("assign-role")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
        {
            await _assignRoleHandler.Handle(command);
            return Ok(ApiResponse<object>.Ok(null, "Role assigned successfully."));
        }
    }
}