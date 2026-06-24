using WarehouseManagementSystem.Domain.Common;

namespace WarehouseManagementSystem.Domain.Entities
{
    public class Inventory : AuditableEntity
    {
        public int Quantity { get; set; }
        public int MinimumStockLevel { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid LocationId { get; set; }
        public Location Location { get; set; } = null!;
    }
}