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

        // Always positive — type determines direction
        public int Quantity { get; set; }

        // Optional reference e.g. PO number, order number
        public string? Reference { get; set; }
        public string? Notes { get; set; }

        // Who registered this movement
        public Guid CreatedBy { get; set; }
    }
}