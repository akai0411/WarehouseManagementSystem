using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Inventory.Common;

namespace WarehouseManagementSystem.Application.Features.Inventory.GetInventoryById
{
    public class GetInventoryByIdHandler
    {
        private readonly IInventoryRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetInventoryByIdHandler(
            IInventoryRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<InventoryDto?> Handle(Guid id)
        {
            var inventory = await _repository.GetByIdAsync(id);

            if (inventory == null) return null;

            var warehouseId = inventory.Location.Zone.WarehouseId;

            // WarehouseManager and Operator can only see their warehouse
            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                if (warehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You do not have access to this inventory.");
            }

            // RegionalManager can only see their country
            if (_currentUser.IsRegionalManager)
            {
                if (inventory.Location.Zone.Warehouse.Address.Country != _currentUser.Country)
                    throw new UnauthorizedException(
                        "You do not have access to this inventory.");
            }

            return InventoryMapper.ToDto(inventory);
        }
    }
}