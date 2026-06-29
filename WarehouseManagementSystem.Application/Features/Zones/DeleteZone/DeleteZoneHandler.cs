using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Zones.DeleteZone
{
    public class DeleteZoneHandler
    {
        private readonly IZoneRepository _repository;

        public DeleteZoneHandler(IZoneRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(Guid warehouseId, Guid zoneId)
        {
            var zone = await _repository.GetByIdAsync(zoneId);

            if (zone == null || zone.WarehouseId != warehouseId) return false;

            var hasActiveLocations = await _repository.HasActiveLocationsAsync(zoneId);
            if (hasActiveLocations)
                throw new BusinessRuleException(
                    "Cannot delete a zone that has active locations. " +
                    "Please remove all locations first.");

            await _repository.DeleteAsync(zone);

            return true;
        }
    }
}