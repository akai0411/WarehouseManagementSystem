namespace WarehouseManagementSystem.Application.Features.Zones.Common
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}