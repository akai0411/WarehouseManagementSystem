using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Locations.GetLocations;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Locations
{
    public class GetLocationsHandlerTests
    {
        private readonly Mock<ILocationRepository> _locationRepository = new();
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();

        private GetLocationsHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_locationRepository.Object, _zoneRepository.Object, _warehouseRepository.Object,
                currentUser.Object);

        [Fact]
        public async Task Handle_ZoneNotFound_ThrowsNotFoundException()
        {
            _zoneRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Zone?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(Guid.NewGuid(), new GetLocationsQuery()));
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(zone.Id, new GetLocationsQuery()));
        }

        [Fact]
        public async Task Handle_ComputesTotalPagesFromTotalCountAndPageSize()
        {
            var zone = EntityFactory.CreateZone(EntityFactory.CreateWarehouse());
            _zoneRepository.Setup(r => r.GetByIdAsync(zone.Id)).ReturnsAsync(zone);
            _locationRepository.Setup(r => r.GetAllAsync(zone.Id, It.IsAny<LocationFilter>()))
                .ReturnsAsync((new List<Location>(), 25));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(zone.Id, new GetLocationsQuery { PageSize = 20 });

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.TotalPages); // ceil(25/20)
        }
    }
}
