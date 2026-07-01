using Moq;
using WarehouseManagementSystem.Application.Common.Filters;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Warehouses
{
    public class GetWarehousesHandlerTests
    {
        private readonly Mock<IWarehouseRepository> _repository = new();

        private GetWarehousesHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_repository.Object, currentUser.Object);

        [Fact]
        public async Task Handle_ComputesTotalPagesFromTotalCountAndPageSize()
        {
            _repository.Setup(r => r.GetAllAsync(It.IsAny<WarehouseFilter>()))
                .ReturnsAsync((new List<Warehouse>(), 25));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());
            var result = await handler.Handle(new GetWarehousesQuery { PageSize = 20 });

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.TotalPages); // ceil(25/20)
        }

        [Fact]
        public async Task Handle_RegionalManager_OverridesCountryFilterWithOwnCountry()
        {
            // A RegionalManager must only ever see their own country's
            // warehouses, even if they try to pass a different Country in
            // the query string.
            _repository
                .Setup(r => r.GetAllAsync(It.Is<WarehouseFilter>(f => f.Country == "Spain")))
                .ReturnsAsync((new List<Warehouse>(), 0));

            var handler = CreateHandler(CurrentUserMockFactory.RegionalManager("Spain"));

            await handler.Handle(new GetWarehousesQuery { Country = "France" });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<WarehouseFilter>(f => f.Country == "Spain")), Times.Once);
        }

        [Fact]
        public async Task Handle_Admin_UsesQueryCountryAsIs()
        {
            _repository
                .Setup(r => r.GetAllAsync(It.IsAny<WarehouseFilter>()))
                .ReturnsAsync((new List<Warehouse>(), 0));

            var handler = CreateHandler(CurrentUserMockFactory.Admin());

            await handler.Handle(new GetWarehousesQuery { Country = "France" });

            _repository.Verify(r => r.GetAllAsync(
                It.Is<WarehouseFilter>(f => f.Country == "France")), Times.Once);
        }
    }
}
