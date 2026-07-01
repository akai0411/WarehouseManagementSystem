using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Inventory.DeleteInventory
{
    public class DeleteInventoryHandler
    {
        private readonly IInventoryRepository _repository;

        public DeleteInventoryHandler(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(Guid id)
        {
            var inventory = await _repository.GetByIdAsync(id);

            if (inventory == null) return false;

            // Cannot unassign a location that still has stock
            if (inventory.Quantity > 0)
                throw new BusinessRuleException(
                    "Cannot unassign a product from a location that still has stock. " +
                    "Please register an outbound movement to clear stock first.");

            await _repository.DeleteAsync(inventory);

            return true;
        }
    }
}