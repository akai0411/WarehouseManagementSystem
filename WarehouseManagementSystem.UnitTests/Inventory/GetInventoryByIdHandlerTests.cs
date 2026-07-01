using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventoryById;
using WarehouseManagementSystem.UnitTests.TestHelpers;
using InventoryEntity = WarehouseManagementSystem.Domain.Entities.Inventory;

namespace WarehouseManagementSystem.UnitTests.Inventory
{
    public class GetInventoryByIdHandlerTests
    {
        private readonly Mock<IInventoryRepository> _repository = new();

        private GetInventoryByIdHandler CreateHandler(Mock<ICurrentUserService> currentUser)
        {
            return new GetInventoryByIdHandler(_repository.Object, currentUser.Object);
        }

        [Fact]
        public async Task Handle_InventoryNotFound_ReturnsNull()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((InventoryEntity?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_Admin_ReturnsDtoRegardlessOfWarehouse()
        {
            var inventory = EntityFactory.CreateInventory();
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(inventory.Id);

            Assert.NotNull(result);
            Assert.Equal(inventory.Id, result!.Id);
        }

        [Fact]
        public async Task Handle_WarehouseManagerSameWarehouse_ReturnsDto()
        {
            var warehouseId = Guid.NewGuid();
            var inventory = EntityFactory.CreateInventory(warehouseId: warehouseId);
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(warehouseId));
            var result = await handler.Handle(inventory.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var inventory = EntityFactory.CreateInventory(warehouseId: Guid.NewGuid());
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(inventory.Id));
        }

        [Fact]
        public async Task Handle_OperatorDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var inventory = EntityFactory.CreateInventory(warehouseId: Guid.NewGuid());
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.Operator(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(inventory.Id));
        }

        [Fact]
        public async Task Handle_RegionalManagerSameCountry_ReturnsDto()
        {
            var inventory = EntityFactory.CreateInventory(country: "Spain");
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("Spain"));
            var result = await handler.Handle(inventory.Id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Handle_RegionalManagerDifferentCountry_ThrowsUnauthorizedException()
        {
            var inventory = EntityFactory.CreateInventory(country: "Spain");
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("France"));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(inventory.Id));
        }

        [Fact]
        public async Task Handle_RegionalManagerWithNoCountry_ThrowsUnauthorizedException()
        {
            // Fails closed: a warehouse's real country will never equal a
            // null Country claim, so this already denies access rather than
            // silently granting it.
            var inventory = EntityFactory.CreateInventory(country: "Spain");
            _repository.Setup(r => r.GetByIdAsync(inventory.Id)).ReturnsAsync(inventory);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager(country: null));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(inventory.Id));
        }
    }
}
