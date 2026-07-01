using Moq;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.UnitTests.TestHelpers
{
    /// <summary>
    /// Builds a mocked ICurrentUserService for a given role, so handler
    /// tests can exercise role-based scoping/authorization without a real
    /// HttpContext or JWT.
    /// </summary>
    internal static class CurrentUserMockFactory
    {
        public static Mock<ICurrentUserService> Create(
            string role,
            Guid? userId = null,
            Guid? warehouseId = null,
            string? country = null)
        {
            var mock = new Mock<ICurrentUserService>();

            mock.SetupGet(u => u.UserId).Returns(userId ?? Guid.NewGuid());
            mock.SetupGet(u => u.Role).Returns(role);
            mock.SetupGet(u => u.WarehouseId).Returns(warehouseId);
            mock.SetupGet(u => u.Country).Returns(country);
            mock.SetupGet(u => u.IsAdmin).Returns(role == "Admin");
            mock.SetupGet(u => u.IsWarehouseManager).Returns(role == "WarehouseManager");
            mock.SetupGet(u => u.IsRegionalManager).Returns(role == "RegionalManager");
            mock.SetupGet(u => u.IsOperator).Returns(role == "Operator");

            return mock;
        }

        public static Mock<ICurrentUserService> Admin() => Create("Admin");

        public static Mock<ICurrentUserService> WarehouseManager(Guid warehouseId) =>
            Create("WarehouseManager", warehouseId: warehouseId);

        public static Mock<ICurrentUserService> Operator(Guid warehouseId) =>
            Create("Operator", warehouseId: warehouseId);

        public static Mock<ICurrentUserService> RegionalManager(string? country) =>
            Create("RegionalManager", country: country);
    }
}
