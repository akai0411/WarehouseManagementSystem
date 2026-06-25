using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Warehouses.Common;

namespace WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouseById
{
    public class GetWarehouseByIdHandler
    {
        private readonly IWarehouseRepository _repository;
        private readonly ICurrentUserService _currentUser;
        public GetWarehouseByIdHandler(
            IWarehouseRepository repository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<WarehouseDto?> Handle(Guid id)
        {
            var warehouse = await _repository.GetByIdAsync(id);

            if (warehouse == null) return null;

            // WarehouseManager can only see their own warehouse
            if (_currentUser.IsWarehouseManager &&
                warehouse.Id != _currentUser.WarehouseId)
                throw new UnauthorizedException(
                    "You do not have access to this warehouse.");


            return WarehouseMapper.ToDto(warehouse);
        }
    }
}