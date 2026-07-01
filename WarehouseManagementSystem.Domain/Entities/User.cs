using WarehouseManagementSystem.Domain.Common;

namespace WarehouseManagementSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "Operator";
        public Guid? WarehouseId { get; set; }
        public string? Country { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}