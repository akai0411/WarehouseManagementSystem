using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Zones.GetZones;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Zones
{
    public class GetZonesHandlerTests
    {
        private readonly Mock<IZoneRepository> _zoneRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();

        private GetZonesHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_zoneRepository.Object, _warehouseRepository.Object, currentUser.Object);

        [Fact]
        public async Task Handle_WarehouseNotFound_ThrowsNotFoundException()
        {
            _warehouseRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Warehouse?)null);

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(Guid.NewGuid(), new GetZonesQuery()));
        }

        [Fact]
        public async Task Handle_WarehouseManagerDifferentWarehouse_ThrowsUnauthorizedException()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(Guid.NewGuid()));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(warehouse.Id, new GetZonesQuery()));
        }

        [Fact]
        public async Task Handle_ComputesTotalPagesFromTotalCountAndPageSize()
        {
            var warehouse = EntityFactory.CreateWarehouse();
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);
            _zoneRepository.Setup(r => r.GetAllAsync(warehouse.Id, It.IsAny<ZoneFilter>()))
                .ReturnsAsync((new List<Zone>(), 25));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(warehouse.Id, new GetZonesQuery { PageSize = 20 });

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.TotalPages); // ceil(25/20)
        }
    }
}
