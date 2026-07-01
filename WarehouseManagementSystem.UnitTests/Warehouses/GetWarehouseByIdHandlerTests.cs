using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouseById;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Warehouses
{
    public class GetWarehouseByIdHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repository = new();

        private GetWarehouseByIdHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_repository.Object, currentUser.Object);

        [Fact]
        public async Task Handle_WarehouseNotFound_ReturnsNull()
        {
            _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Warehouse?)null);

            var result = await CreateHandler(CurrentUserMockFactory.Admin()).Handle(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_WarehouseManagerRequestingOwnWarehouse_ReturnsDto()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(warehouse.Id));
            var result = await handler.Handle(warehouse.Id);

            Assert.NotNull(result);
            Assert.Equal(warehouse.Id, result!.Id);
        }

        [Fact]
        public async Task Handle_WarehouseManagerRequestingDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(warehouse.Id));
        }

        [Fact]
        public async Task Handle_Admin_CanAccessAnyWarehouse()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _repository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(warehouse.Id);

            Assert.NotNull(result);
        }
    }
}
