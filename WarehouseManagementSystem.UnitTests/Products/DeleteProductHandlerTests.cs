using Moq;
using Xunit;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.DeleteProduct;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Products;

public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ProductExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Keyboard",
            SKU = "KB-001",
            Description = "Mechanical keyboard",
            Price = 100
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(r => r.DeleteAsync(product))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(product.Id);

        // Assert
        Assert.True(result);

        _repositoryMock.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(id);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }
}