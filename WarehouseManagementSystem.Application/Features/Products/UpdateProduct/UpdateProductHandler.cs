

using System.Xml.Linq;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Products.UpdateProduct
{
    public class UpdateProductHandler
    {
        private readonly IProductRepository _repository;

        public UpdateProductHandler(IProductRepository repository)
        {  _repository = repository; }
        
        public async Task<bool> Handle(Guid id, UpdateProductCommand request)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null) return false;

            product.Name = request.Name;
            product.SKU = request.SKU;
            product.Description = request.Description;
            product.Price = request.Price;
            product.QuantityInStock = request.QuantityInStock;
            product.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(product);

            return true;
        }

    }
}
