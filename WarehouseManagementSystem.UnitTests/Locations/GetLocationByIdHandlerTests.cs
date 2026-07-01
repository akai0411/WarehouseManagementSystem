using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Locations.GetLocationById;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Locations
{
    public class GetLocationByIdHandlerTests
    {
        private readonly Mock<ILocationRepository> _locationRepository = new();
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();

        private GetLocationByIdHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_locationRepository.Object, _zoneRepository.Object, _warehouseRepository.Object,
                currentUser.Object);

        [Fact]
        public async Task Handle_ZoneNotFound_ThrowsNotFoundException()
        {
            _zoneRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(zone.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_RegionalManagerDifferentCountry_ThrowsUnauthorizedException()
        {
            var warehouse = EntityFactory.CreateWarehouse(country: "Spain");
            var zone = EntityFactory.CreateZone(warehouse);
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("France"));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(zone.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task Handle_LocationNotFound_ReturnsNull()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Location?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(zone.Id, Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_LocationBelongsToDifferentZone_ReturnsNull()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var locationInOtherZone = EntityFactory.CreateLocation(
                EntityFactory.CreateZone(EntityFactory.CreateWarehouse()));
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetByIdAsync(locationInOtherZone.Id))
                .ReturnsAsync(locationInOtherZone);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(zone.Id, locationInOtherZone.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsDto()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            var location = EntityFactory.CreateLocation(zone);
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetByIdAsync(location.Id)).ReturnsAsync(location);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(zone.Id, location.Id);

            Assert.NotNull(result);
            Assert.Equal(location.Id, result!.Id);
        }
    }
}
