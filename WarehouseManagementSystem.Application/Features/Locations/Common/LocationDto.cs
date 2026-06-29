namespace WarehouseManagementSystem.Application.Features.Locations.Common
{
    public class LocationDto
    {
        public Guid Id { get; set; }
        public string Row { get; set; } = string.Empty;
        public int Shelf { get; set; }
        public int Bin { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid ZoneId { get; set; }
        public bool HasInventory { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}