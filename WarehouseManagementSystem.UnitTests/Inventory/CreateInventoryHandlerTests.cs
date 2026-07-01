using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Inventory.CreateInventory;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Inventory
{
    public class CreateInventoryHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepository = new();
        private readonly Mock<IProductRepository> _productRepository = new();
        private readonly Mock<ILocationRepository> _locationRepository = new();
        private readonly CreateInventoryCommandValidator _validator = new();

        private CreateInventoryHandler CreateHandler(Mock<ICurrentUserService> currentUser)
        {
            return new CreateInventoryHandler(
                _inventoryRepository.Object,
                _productRepository.Object,
                _locationRepository.Object,
                currentUser.Object,
                _validator);
        }

        [Fact]
        public async Task Handle_ValidCommand_AssignsProductAndReturnsNewInventoryId()
        {
            var product = EntityFactory.CreateProduct();
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            var location = EntityFactory.CreateLocation(zone);

            _productRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _locationRepository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _inventoryRepository.Setup(r => r.GetByLocationIdAsync(location.Id))
                .ReturnsAsync((WarehouseManagementSystem.Domain.Entities.Inventory?)null);
            // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
            // to any entity whose Id is still Guid.Empty when persisted.
            _inventoryRepository.Setup(r => r.AddAsync(It.IsAny<WarehouseManagementSystem.Domain.Entities.Inventory>()))
                .Callback<WarehouseManagementSystem.Domain.Entities.Inventory>(i => i.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            var command = new CreateInventoryCommand
            {
                ProductId = product.Id,
                LocationId = location.Id,
                Quantity = 0,
                MinimumStockLevel = 5
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            var resultId = await handler.Handle(command);

            Assert.NotEqual(Guid.Empty, resultId);
            _inventoryRepository.Verify(r => r.AddAsync(It.Is<WarehouseManagementSystem.Domain.Entities.Inventory>(
                i => i.ProductId == product.Id &&
                     i.LocationId == location.Id &&
                     i.MinimumStockLevel == 5 &&
                     i.Quantity == 0)), Times.Once);
        }

        [Fact]
        public async Task Handle_ProductDoesNotExist_ThrowsNotFoundException()
        {
            var location = EntityFactory.CreateLocation(
                EntityFactory.CreateZone(EntityFactory.CreateWarehouse()));

            _productRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Product?)null);

            var command = new CreateInventoryCommand
            {
                ProductId = Guid.NewGuid(),
                LocationId = location.Id,
                Quantity = 0,
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command));
            _inventoryRepository.Verify(r => r.AddAsync(It.IsAny<WarehouseManagementSystem.Domain.Entities.Inventory>()), Times.Never);
        }

        [Fact]
        public async Task Handle_LocationDoesNotExist_ThrowsNotFoundException()
        {
            var product = EntityFactory.CreateProduct();
            _productRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _locationRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Location?)null);

            var command = new CreateInventoryCommand
            {
                ProductId = product.Id,
                LocationId = Guid.NewGuid(),
                Quantity = 0,
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_LocationAlreadyOccupied_ThrowsConflictException()
        {
            var product = EntityFactory.CreateProduct();
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            var location = EntityFactory.CreateLocation(zone);
            var existingInventory = EntityFactory.CreateInventory();

            _productRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _locationRepository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _inventoryRepository.Setup(r => r.GetByLocationIdAsync(location.Id))
                .ReturnsAsync(existingInventory);

            var command = new CreateInventoryCommand
            {
                ProductId = product.Id,
                LocationId = location.Id,
                Quantity = 0,
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command));
            _inventoryRepository.Verify(r => r.AddAsync(It.IsAny<WarehouseManagementSystem.Domain.Entities.Inventory>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WarehouseManagerAssigningToOtherWarehouse_ThrowsUnauthorizedException()
        {
            var product = EntityFactory.CreateProduct();
            var ownWarehouseId = Guid.NewGuid();
            var otherWarehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(otherWarehouse);
            var location = EntityFactory.CreateLocation(zone);

            _productRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _locationRepository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);

            var command = new CreateInventoryCommand
            {
                ProductId = product.Id,
                LocationId = location.Id,
                Quantity = 0,
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_WarehouseManagerAssigningWithinOwnWarehouse_Succeeds()
        {
            var product = EntityFactory.CreateProduct();
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            var location = EntityFactory.CreateLocation(zone);

            _productRepository.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _locationRepository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);
            _inventoryRepository.Setup(r => r.GetByLocationIdAsync(location.Id))
                .ReturnsAsync((WarehouseManagementSystem.Domain.Entities.Inventory?)null);
            // Simulates AppDbContext.SaveChangesAsync, which assigns a new Id
            // to any entity whose Id is still Guid.Empty when persisted.
            _inventoryRepository.Setup(r => r.AddAsync(It.IsAny<WarehouseManagementSystem.Domain.Entities.Inventory>()))
                .Callback<WarehouseManagementSystem.Domain.Entities.Inventory>(i => i.Id = Guid.NewGuid())
                .Returns(Task.CompletedTask);

            var command = new CreateInventoryCommand
            {
                ProductId = product.Id,
                LocationId = location.Id,
                Quantity = 0,
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(warehouse.Id));

            var resultId = await handler.Handle(command);

            Assert.NotEqual(Guid.Empty, resultId);
        }

        [Fact]
        public async Task Handle_InitialQuantityNotZero_ThrowsValidationException()
        {
            var command = new CreateInventoryCommand
            {
                ProductId = Guid.NewGuid(),
                LocationId = Guid.NewGuid(),
                Quantity = 10, // must start at 0 — stock is added via movements
                MinimumStockLevel = 0
            };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command));
            _productRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
