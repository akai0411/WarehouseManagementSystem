
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Products.DeleteProduct
{
    public class DeleteProductHandler
    {
        private readonly IProductRepository _repository;

        public DeleteProductHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(Guid id)
        {
            var product = await
              _repository.GetByIdAsync(id);

            if (product == null) return false;

            await _repository.DeleteAsync(product);

            return true;
        }
    }
}
