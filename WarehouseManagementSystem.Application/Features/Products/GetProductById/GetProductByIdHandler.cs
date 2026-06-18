using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.Application.Features.Products.Common;


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

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                QuantityInStock = product.QuantityInStock
            };

        }
    }
}
