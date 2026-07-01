using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Products;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;

    // Uses the real validator (same pattern as the Zone/Warehouse/Location
    // Create*HandlerTests) instead of mocking IValidator, so the actual
    // FluentValidation rules (ValidName/ValidPrice/ValidDescription) are
    // genuinely exercised here rather than just assumed to work.
    private readonly UpdateProductCommandValidator _validator = new();
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new UpdateProductHandler(_repositoryMock.Object, _validator);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesProductAndReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalRowVersion = new byte[] { 1, 2, 3, 4 };

        var command = new UpdateProductCommand
        {
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1800,
            RowVersion = originalRowVersion
        };

        var product = new Product
        {
            Id = id,
            Name = "Laptop",
            Description = "Old Description",
            Price = 1500
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<byte[]>()))
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

        // Confirms the client's originally-read RowVersion is the one forwarded
        // to the repository — that's what makes the concurrency check meaningful
        // (see Handle_ConcurrentModification_ThrowsConflictException below).
        _repositoryMock.Verify(
            r => r.UpdateAsync(product, originalRowVersion), Times.Once);
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

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(id, command);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<byte[]>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Triggers the real ValidName rule (NotEmpty) — Name defaults to "".
        var id = Guid.NewGuid();
        var command = new UpdateProductCommand();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(id, command));

        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConcurrentModification_ThrowsConflictException()
    {
        // Simulates another user having changed the product between when this
        // caller originally read it (command.RowVersion) and this request's
        // UpdateAsync call — exactly the scenario RowVersion is meant to catch.
        var id = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Name = "Updated Laptop",
            Description = "Updated Description",
            Price = 1800,
            RowVersion = new byte[] { 1, 2, 3, 4 } // stale token
        };

        var product = new Product
        {
            Id = id,
            Name = "Laptop",
            Description = "Old Description",
            Price = 1500
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<byte[]>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(id, command));
    }
}