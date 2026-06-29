namespace WarehouseManagementSystem.Application.Features.Zones.UpdateZone
{
    public class UpdateZoneCommand
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}  