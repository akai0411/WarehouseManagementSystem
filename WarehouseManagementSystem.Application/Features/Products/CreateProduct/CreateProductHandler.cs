using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;


namespace WarehouseManagementSystem.Application.Features.Products.CreateProduct
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _repository;

        public CreateProductHandler(IProductRepository repository)
        {
            _repository = repository;
        }
        public async Task<Guid> Handle(CreateProductCommand request)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SKU = request.SKU,
                Description = request.Description,
                Price = request.Price,
                QuantityInStock = request.QuantityInStock,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(product);
            return product.Id;
        }

    }
}
