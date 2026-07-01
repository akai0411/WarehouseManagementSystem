using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Zones.GetZoneById;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Zones
{
    public class GetZoneByIdHandlerTests
    {
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();

        private GetZoneByIdHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_zoneRepository.Object, _warehouseRepository.Object, currentUser.Object);

        [Fact]
        public async Task Handle_WarehouseNotFound_ThrowsNotFoundException()
        {
            _warehouseRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Warehouse?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(warehouse.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_RegionalManagerDifferentCountry_ThrowsUnauthorizedException()
        {
            var warehouse = EntityFactory.CreateWarehouse(country: "Spain");
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("France"));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(warehouse.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_ZoneNotFound_ReturnsNull()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(warehouse.Id, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ZoneBelongsToDifferentWarehouse_ReturnsNull()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zoneInOtherWarehouse = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetByIdAsync(zoneInOtherWarehouse.Id))
                .ReturnsAsync(zoneInOtherWarehouse);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(warehouse.Id, zoneInOtherWarehouse.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsDto()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            var zone = EntityFactory.CreateZone(warehouse);
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(warehouse.Id, zone.Id);

            Assert.NotNull(result);
            Assert.Equal(zone.Id, result!.Id);
        }
    }
}
