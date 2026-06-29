namespace WarehouseManagementSystem.Application.Features.Warehouses.Common
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AddressDto Address { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class AddressDto
    {
        public string StreetType { get; set; } = string.Empty;
        public string StreetName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }
}