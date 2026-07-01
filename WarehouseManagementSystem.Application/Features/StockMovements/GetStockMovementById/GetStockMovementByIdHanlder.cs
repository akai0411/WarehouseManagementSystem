using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.StockMovements.Common;

namespace WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovementById
{
    public class GetStockMovementByIdHandler
    {
        private readonly IStockMovementRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetStockMovementByIdHandler(
            IStockMovementRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<StockMovementDto?> Handle(Guid id)
        {
            var movement = await _repository.GetByIdAsync(id);

            if (movement == null) return null;

            var warehouseId = movement.Inventory.Location.Zone.WarehouseId;

            if (_currentUser.IsWarehouseManager || _currentUser.IsOperator)
            {
                if (warehouseId != _currentUser.WarehouseId)
                    throw new UnauthorizedException(
                        "You do not have access to this movement.");
            }

            if (_currentUser.IsRegionalManager)
            {
                if (movement.Inventory.Location.Zone.Warehouse.Address.Country
                    != _currentUser.Country)
                    throw new UnauthorizedException(
                        "You do not have access to this movement.");
            }

            return StockMovementMapper.ToDto(movement);
        }
    }
}