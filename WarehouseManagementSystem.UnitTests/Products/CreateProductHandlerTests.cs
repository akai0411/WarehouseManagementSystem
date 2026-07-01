using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Products;
public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IValidator<CreateProductCommand>> _validator = new();

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewProductId()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            SKU = "SKU-1",
            Price = 10
        };

        _validator.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
        // to any entity whose Id is still Guid.Empty when persisted.
        _repo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = Guid.NewGuid())
            .Returns(Task.CompletedTask);

        var handler = new CreateProductHandler(_repo.Object, _validator.Object);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsConflictException()
    {
        var command = new CreateProductCommand
        {
            Name = "Test",
            SKU = "DUPLICATE",
            Price = 10
        };

        _validator
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repo
        .Setup(r => r.GetBySkuAsync("DUPLICATE"))
        .ReturnsAsync(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Existing Product",
            SKU = "DUPLICATE",
            Price = 10
        });

        var handler = new CreateProductHandler(_repo.Object, _validator.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(command));
    }
}