namespace WarehouseManagementSystem.Application.Common.Filters
{
    public class StockMovementFilter
    {
        public Guid? InventoryId { get; set; }
        public Guid? WarehouseId { get; set; }
        public Guid? ProductId { get; set; }
        public string? Type { get; set; }
        public string? Country { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}