using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Warehouses.DeleteWarehouse
{
    public class DeleteWarehouseHandler
    {
        private readonly IWarehouseRepository _repository;

        public DeleteWarehouseHandler(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(Guid id)
        {
            var warehouse = await _repository.GetByIdAsync(id);

            if (warehouse == null) return false;

            // Mirrors DeleteZoneHandler/DeleteLocationHandler: a warehouse
            // shouldn't be soft-deleted while it still has active zones under
            // it, otherwise those zones would be left orphaned under a
            // deleted warehouse.
            var hasActiveZones = await _repository.HasActiveZonesAsync(id);
            if (hasActiveZones)
                throw new BusinessRuleException(
                    "Cannot delete a warehouse that has active zones. " +
                    "Please remove all zones first.");

            await _repository.DeleteAsync(warehouse);

            return true;
        }
    }
}