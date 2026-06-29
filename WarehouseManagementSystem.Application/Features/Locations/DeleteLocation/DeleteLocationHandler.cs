using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Locations.DeleteLocation
{
    public class DeleteLocationHandler
    {
        private readonly ILocationRepository _repository;

        public DeleteLocationHandler(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(Guid zoneId, Guid locationId)
        {
            var location = await _repository.GetByIdAsync(locationId);

            if (location == null || location.ZoneId != zoneId) return false;

            var hasInventory = await _repository.HasInventoryAsync(locationId);
            if (hasInventory)
                throw new BusinessRuleException(
                    "Cannot delete a location that has inventory assigned. " +
                    "Please remove the inventory first.");

            await _repository.DeleteAsync(location);

            return true;
        }
    }
}