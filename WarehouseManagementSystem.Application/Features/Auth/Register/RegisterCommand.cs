namespace WarehouseManagementSystem.Application.Features.Auth.Register
{
    public class RegisterCommand
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
