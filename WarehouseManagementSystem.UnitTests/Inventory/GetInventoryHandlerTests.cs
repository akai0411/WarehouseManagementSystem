using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventory;
using WarehouseManagementSystem.UnitTests.TestHelpers;
using InventoryEntity = WarehouseManagementSystem.Domain.Entities.Inventory;

namespace WarehouseManagementSystem.UnitTests.Inventory
{
    public class GetInventoryHandlerTests
    {
        private readonly Mock<IInventoryRepository> _repository = new();

        private GetInventoryHandler CreateHandler(Mock<ICurrentUserService> currentUser)
        {
            return new GetInventoryHandler(_repository.Object, currentUser.Object);
        }

        [Fact]
        public async Task Handle_Admin_PassesThroughRequestedWarehouseId()
        {
            InventoryFilter? captured = null;
            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .Callback<InventoryFilter>(f => captured = f)
                .ReturnsAsync((new List<InventoryEntity>(), 0));

            var requestedWarehouseId = Guid.NewGuid();
            var query = new GetInventoryQuery { WarehouseId = requestedWarehouseId };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            await handler.Handle(query);

            Assert.Equal(requestedWarehouseId, captured!.WarehouseId);
            Assert.Null(captured.Country);
        }

        [Fact]
        public async Task Handle_WarehouseManager_ForcesFilterToOwnWarehouseIgnoringQuery()
        {
            InventoryFilter? captured = null;
            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .Callback<InventoryFilter>(f => captured = f)
                .ReturnsAsync((new List<InventoryEntity>(), 0));

            var ownWarehouseId = Guid.NewGuid();
            var query = new GetInventoryQuery { WarehouseId = Guid.NewGuid() }; // some other warehouse

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));
            await handler.Handle(query);

            Assert.Equal(ownWarehouseId, captured!.WarehouseId);
        }

        [Fact]
        public async Task Handle_Operator_ForcesFilterToOwnWarehouseIgnoringQuery()
        {
            InventoryFilter? captured = null;
            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .Callback<InventoryFilter>(f => captured = f)
                .ReturnsAsync((new List<InventoryEntity>(), 0));

            var ownWarehouseId = Guid.NewGuid();
            var query = new GetInventoryQuery { WarehouseId = Guid.NewGuid() };

            var handler = CreateHandler(CurrentUserMockFactory.Operator(ownWarehouseId));
            await handler.Handle(query);

            Assert.Equal(ownWarehouseId, captured!.WarehouseId);
        }

        [Fact]
        public async Task Handle_RegionalManager_SetsCountryAndAllowsWarehouseIdFromQuery()
        {
            InventoryFilter? captured = null;
            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .Callback<InventoryFilter>(f => captured = f)
                .ReturnsAsync((new List<InventoryEntity>(), 0));

            var requestedWarehouseId = Guid.NewGuid();
            var query = new GetInventoryQuery { WarehouseId = requestedWarehouseId };

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("Spain"));
            await handler.Handle(query);

            Assert.Equal(requestedWarehouseId, captured!.WarehouseId);
            Assert.Equal("Spain", captured.Country);
        }

        [Fact]
        public async Task Handle_RegionalManagerWithNoCountry_ThrowsUnauthorizedInsteadOfSeeingEverything()
        {
            var query = new GetInventoryQuery();

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager(country: null));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(query));
            _repository.Verify(r => r.GetAllAsync(It.IsAny<InventoryFilter>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ComputesTotalPagesFromTotalCountAndPageSize()
        {
            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .ReturnsAsync((new List<InventoryEntity>(), 25));

            var query = new GetInventoryQuery { PageNumber = 1, PageSize = 20 };

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(query);

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.TotalPages); // ceil(25/20)
        }

        [Fact]
        public async Task Handle_MapsIsLowStockFromQuantityAndMinimumStockLevel()
        {
            var lowStockItem = EntityFactory.CreateInventory(quantity: 2, minimumStockLevel: 5);
            var healthyItem = EntityFactory.CreateInventory(quantity: 20, minimumStockLevel: 5);

            _repository.Setup(r => r.GetAllAsync(It.IsAny<InventoryFilter>()))
                .ReturnsAsync((new List<InventoryEntity> { lowStockItem, healthyItem }, 2));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(new GetInventoryQuery());

            Assert.True(result.Items.Single(i => i.Id == lowStockItem.Id).IsLowStock);
            Assert.False(result.Items.Single(i => i.Id == healthyItem.Id).IsLowStock);
        }
    }
}
