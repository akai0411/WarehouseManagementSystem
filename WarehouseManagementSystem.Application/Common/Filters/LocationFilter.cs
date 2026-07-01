namespace WarehouseManagementSystem.Application.Common.Filters
{
    public class LocationFilter
    {
        public string? Row { get; set; }
        public bool? HasInventory { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}