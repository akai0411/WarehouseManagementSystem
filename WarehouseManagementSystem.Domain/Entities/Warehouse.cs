using WarehouseManagementSystem.Domain.Common;
using WarehouseManagementSystem.Domain.ValueObjects;

namespace WarehouseManagementSystem.Domain.Entities
{
    public class Warehouse : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Address Address { get; set; } = null!;
        public ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}