using Moq;
using Xunit;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Products;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ProductsExist_ReturnsPagedResponse()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                SKU = "LP-001",
                Description = "Gaming Laptop",
                Price = 1500
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Mouse",
                SKU = "MS-001",
                Description = "Wireless Mouse",
                Price = 50
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(query))
            .ReturnsAsync((products, products.Count));

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        var first = result.Items.First();

        Assert.Equal(products[0].Id, first.Id);
        Assert.Equal(products[0].Name, first.Name);
        Assert.Equal(products[0].SKU, first.SKU);
    }

    [Fact]
    public async Task Handle_NoProducts_ReturnsEmptyPage()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(query))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        var result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }
}