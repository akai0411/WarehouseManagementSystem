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

            await _repository.DeleteAsync(warehouse);

            return true;
        }
    }
}