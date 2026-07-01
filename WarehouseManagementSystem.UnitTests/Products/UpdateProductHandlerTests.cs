using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Products;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IValidator<UpdateProductCommand>> _validatorMock;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _validatorMock = new Mock<IValidator<UpdateProductCommand>>();

        _handler = new UpdateProductHandler(
            _repositoryMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesProductAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1800
        };

        var product = new Product
        {
            Id = id,
            Name = "Laptop",
            Description = "Old Description",
            Price = 1500
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(id, command);

        // Assert
        Assert.True(result);

        Assert.Equal(command.Name, product.Name);
        Assert.Equal(command.Description, product.Description);
        Assert.Equal(command.Price, product.Price);

        // UpdatedAt is intentionally not asserted here
        // it is handled by AppDbContext.SaveChangesAsync
        // and will be covered by integration tests in V2

        _repositoryMock.Verify(r => r.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1800
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(id, command);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();

        var command = new UpdateProductCommand();

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(id, command));
    }
}