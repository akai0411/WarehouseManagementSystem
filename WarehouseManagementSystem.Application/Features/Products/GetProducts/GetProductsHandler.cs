using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.Application.Features.Products.Common;


namespace WarehouseManagementSystem.Application.Features.Products.GetProducts
{
    public class GetProductsHandler
    {
        private readonly IProductRepository _repository;

        public GetProductsHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductDto>> Handle(int page, int pageSize)
        {

            var products = await
                _repository.GetAllPagedAsync(page, pageSize);

            return [.. products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                QuantityInStock = p.QuantityInStock
            })];

        }
    }
}
