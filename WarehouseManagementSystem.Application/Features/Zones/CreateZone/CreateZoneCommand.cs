namespace WarehouseManagementSystem.Application.Features.Zones.CreateZone
{
    public class CreateZoneCommand
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}