using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovements;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.StockMovements
{
    public class GetStockMovementsHandlerTests
    {
        private readonly Mock<IStockMovementRepository> _repository = new();

        private GetStockMovementsHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_repository.Object, currentUser.Object);

        private void SetupEmptyResult()
        {
            _repository.Setup(r => r.GetAllAsync(It.IsAny<StockMovementFilter>()))
                .ReturnsAsync((new List<StockMovement>(), 0));
        }

        [Fact]
        public async Task Handle_WarehouseManager_ScopesToOwnWarehouseIgnoringQuery()
        {
            SetupEmptyResult();
            var ownWarehouseId = Guid.NewGuid();

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));
            await handler.Handle(new GetStockMovementsQuery { WarehouseId = Guid.NewGuid() });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<StockMovementFilter>(f => f.WarehouseId == ownWarehouseId)), Times.Once);
        }

        [Fact]
        public async Task Handle_Operator_ScopesToOwnWarehouseIgnoringQuery()
        {
            SetupEmptyResult();
            var ownWarehouseId = Guid.NewGuid();

            var handler = CreateHandler(CurrentUserMockFactory.Operator(ownWarehouseId));
            await handler.Handle(new GetStockMovementsQuery { WarehouseId = Guid.NewGuid() });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<StockMovementFilter>(f => f.WarehouseId == ownWarehouseId)), Times.Once);
        }

        [Fact]
        public async Task Handle_RegionalManagerWithCountry_ScopesByCountryAndPassesQueryWarehouseId()
        {
            SetupEmptyResult();
            var queryWarehouseId = Guid.NewGuid();

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("Spain"));
            await handler.Handle(new GetStockMovementsQuery { WarehouseId = queryWarehouseId });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<StockMovementFilter>(f =>
                    f.Country == "Spain" && f.WarehouseId == queryWarehouseId)), Times.Once);
        }

        [Fact]
        public async Task Handle_RegionalManagerWithoutCountry_ThrowsUnauthorizedException()
        {
            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager(null));

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(new GetStockMovementsQuery()));

            _repository.Verify(r => r.GetAllAsync(It.IsAny<StockMovementFilter>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Admin_PassesQueryWarehouseIdAsIs()
        {
            SetupEmptyResult();
            var queryWarehouseId = Guid.NewGuid();

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            await handler.Handle(new GetStockMovementsQuery { WarehouseId = queryWarehouseId });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<StockMovementFilter>(f => f.WarehouseId == queryWarehouseId)), Times.Once);
        }

        [Fact]
        public async Task Handle_ComputesTotalPagesFromTotalCountAndPageSize()
        {
            _repository.Setup(r => r.GetAllAsync(It.IsAny<StockMovementFilter>()))
                .ReturnsAsync((new List<StockMovement>(), 25));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(new GetStockMovementsQuery { PageSize = 20 });

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.TotalPages); // ceil(25/20)
        }
    }
}
