using System.ComponentModel.DataAnnotations;
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

        // Optimistic concurrency token. Without this, two concurrent stock
        // movements against the same inventory row can both read the same
        // Quantity, both pass their validation, and the second save silently
        // overwrites the first (lost update).
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];
    }
}