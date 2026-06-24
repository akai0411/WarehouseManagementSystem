using WarehouseManagementSystem.Domain.Common;

namespace WarehouseManagementSystem.Domain.Entities
{
    public class Location : AuditableEntity
    {
        public string Row { get; set; } = string.Empty;
        public int Shelf { get; set; }
        public int Bin { get; set; }
        public string Code { get; set; } = string.Empty; // e.g. "A-01-03"

        public Guid ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;

        public Inventory? Inventory { get; set; } // null if location is empty
    }
}