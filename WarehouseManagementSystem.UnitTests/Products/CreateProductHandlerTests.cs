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

    // Uses the real validator instead of mocking IValidator, so the actual
    // FluentValidation rules are genuinely exercised by these tests.
    private readonly CreateProductCommandValidator _validator = new();

    private CreateProductHandler CreateHandler() => new(_repo.Object, _validator);

    [Fact]
    public async Task Handle_ValidCommand_CreatesProductAndReturnsNewProductId()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            SKU = "SKU-1",
            Price = 10,
            Description = "A test product"
        };

        // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
        // to any entity whose Id is still Guid.Empty when persisted.
        _repo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = Guid.NewGuid())
            .Returns(Task.CompletedTask);

        // Act
        var result = await CreateHandler().Handle(command);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _repo.Verify(r => r.AddAsync(It.Is<Product>(
            p => p.Name == "Test Product" &&
                 p.SKU == "SKU-1" &&
                 p.Price == 10 &&
                 p.Description == "A test product")), Times.Once);
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

        _repo
            .Setup(r => r.GetBySkuAsync("DUPLICATE"))
            .ReturnsAsync(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                SKU = "DUPLICATE",
                Price = 10
            });

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateHandler().Handle(command));

        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsValidationException()
    {
        // Triggers the real ValidName rule (NotEmpty) from
        // ProductValidationExtensions instead of a hand-scripted failure.
        var command = new CreateProductCommand
        {
            Name = "",
            SKU = "SKU-1",
            Price = 10
        };

        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateHandler().Handle(command));

        _repo.Verify(r => r.GetBySkuAsync(It.IsAny<string>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NegativePrice_ThrowsValidationException()
    {
        // Triggers the real ValidPrice rule (GreaterThan(0)).
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            SKU = "SKU-1",
            Price = -5
        };

        await Assert.ThrowsAsync<ValidationException>(() =>
            CreateHandler().Handle(command));

        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}