using WarehouseManagementSystem.Domain.Common;

namespace WarehouseManagementSystem.Domain.Entities
{
    public enum MovementType
    {
        Inbound,
        Outbound,
        Adjustment
    }

    public class StockMovement : BaseEntity
    {
        public Guid InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public MovementType Type { get; set; }

        // Inbound/Outbound: always positive, Type determines direction.
        // Adjustment: signed delta actually applied (newCount - previousCount),
        // so a cycle count can be negative, positive, or zero.
        public int Quantity { get; set; }

        // Optional reference e.g. PO number, order number
        public string? Reference { get; set; }
        public string? Notes { get; set; }

        // Who registered this movement
        public Guid CreatedBy { get; set; }
    }
}