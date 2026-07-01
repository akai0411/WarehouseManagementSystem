namespace WarehouseManagementSystem.Application.Features.Locations.GetLocations
{
    public class GetLocationsQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Row { get; set; }
        public bool? HasInventory { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
    }
}