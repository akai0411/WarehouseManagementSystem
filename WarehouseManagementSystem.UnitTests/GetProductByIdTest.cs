using Moq;
using Xunit;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.GetProductById;
using WarehouseManagementSystem.Domain.Entities;

public class GetProductByIdTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Should_Return_ProductDto_When_Product_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            SKU = "LP-001",
            Description = "Gaming Laptop",
            Price = 1500
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.SKU, result.SKU);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public async Task Should_Return_Null_When_Product_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(id);

        // Assert
        Assert.Null(result);
    }
}