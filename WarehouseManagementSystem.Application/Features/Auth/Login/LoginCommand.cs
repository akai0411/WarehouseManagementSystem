namespace WarehouseManagementSystem.Application.Features.Auth.Login
{
    public class LoginCommand
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
