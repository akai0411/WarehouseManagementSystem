namespace WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse
{
    public class UpdateWarehouseCommand
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string StreetType { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}