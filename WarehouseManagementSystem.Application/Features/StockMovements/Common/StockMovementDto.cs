namespace WarehouseManagementSystem.Application.Features.StockMovements.Common
{
    public class StockMovementDto
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}