using WarehouseManagementSystem.Domain.Common;

namespace WarehouseManagementSystem.Domain.Entities
{
    public enum ZoneType
    {
        Dry,
        Refrigerated,
        Frozen,
        Hazardous,
        HighValue,
        Bulk,
        Quarantine,
        Returns
    }

    public class Zone : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ZoneType Type { get; set; }

        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}