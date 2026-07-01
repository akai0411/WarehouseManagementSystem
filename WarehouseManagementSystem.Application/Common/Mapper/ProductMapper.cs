using WarehouseManagementSystem.Application.Features.Products.Common;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WarehouseManagementSystem.Application.Common.Mapping;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Description = product.Description,
            Price = product.Price,
            RowVersion = product.RowVersion
        };
    }

    public static Product ToEntity(CreateProductCommand command)
    {
        return new Product
        {
            Name = command.Name,
            SKU = command.SKU,
            Description = command.Description,
            Price = command.Price
        };
    }
}