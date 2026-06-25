namespace WarehouseManagementSystem.Application.Features.Auth.AssignRole
{
    public class AssignRoleCommand
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string? Country { get; set; }
    }
}