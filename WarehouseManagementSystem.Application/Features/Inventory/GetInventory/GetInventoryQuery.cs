namespace WarehouseManagementSystem.Application.Features.Inventory.GetInventory
{
    public class GetInventoryQuery
    {
        public Guid? WarehouseId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? LocationId { get; set; }
        public bool? IsLowStock { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}