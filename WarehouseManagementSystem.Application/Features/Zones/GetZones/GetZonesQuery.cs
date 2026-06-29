namespace WarehouseManagementSystem.Application.Features.Zones.GetZones
{
    public class GetZonesQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
    }
}