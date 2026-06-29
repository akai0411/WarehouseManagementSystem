using WarehouseManagementSystem.Application.Features.Zones.Common;
using WarehouseManagementSystem.Application.Features.Zones.CreateZone;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class ZoneMapper
    {
        public static ZoneDto ToDto(Zone zone)
        {
            return new ZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                Description = zone.Description,
                Type = zone.Type.ToString(),
                WarehouseId = zone.WarehouseId,
                CreatedAt = zone.CreatedAt
            };
        }

        public static Zone ToEntity(CreateZoneCommand command, Guid warehouseId)
        {
            return new Zone
            {
                Name = command.Name,
                Description = command.Description,
                Type = Enum.Parse<ZoneType>(command.Type),
                WarehouseId = warehouseId
            };
        }
    }
}