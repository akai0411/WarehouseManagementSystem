using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Features.Products.Common;
using WarehouseManagementSystem.Domain.Entities;


namespace WarehouseManagementSystem.Application.Features.Products.GetProductById
{
    public class GetProductByIdHandler
    {
        private readonly IProductRepository _repository;

        public GetProductByIdHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductDto?> Handle(Guid id)
        {
            var product = await
                _repository.GetByIdAsync(id);

            if (product == null) return null;

            return ProductMapper.ToDto(product);

        }
    }
}
