using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Warehouses.Common;

namespace WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouseById
{
    public class GetWarehouseByIdHandler
    {
        private readonly IWarehouseRepository _repository;

        public GetWarehouseByIdHandler(IWarehouseRepository repository)
        {
            _repository = repository;
        }

        public async Task<WarehouseDto?> Handle(Guid id)
        {
            var warehouse = await _repository.GetByIdAsync(id);

            if (warehouse == null) return null;

            return WarehouseMapper.ToDto(warehouse);
        }
    }
}