namespace WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses
{
    public class GetWarehousesQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
    }
}