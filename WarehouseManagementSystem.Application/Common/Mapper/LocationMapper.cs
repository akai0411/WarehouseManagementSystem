using WarehouseManagementSystem.Application.Features.Locations.Common;
using WarehouseManagementSystem.Application.Features.Locations.CreateLocation;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class LocationMapper
    {
        public static LocationDto ToDto(Location location)
        {
            return new LocationDto
            {
                Id = location.Id,
                Row = location.Row,
                Shelf = location.Shelf,
                Bin = location.Bin,
                Code = location.Code,
                ZoneId = location.ZoneId,
                HasInventory = location.Inventory != null,
                CreatedAt = location.CreatedAt
            };
        }

        public static Location ToEntity(CreateLocationCommand command, Guid zoneId)
        {
            return new Location
            {
                Row = command.Row,
                Shelf = command.Shelf,
                Bin = command.Bin,
                Code = $"{command.Row}-{command.Shelf:D2}-{command.Bin:D2}",
                ZoneId = zoneId
            };
        }
    }
}