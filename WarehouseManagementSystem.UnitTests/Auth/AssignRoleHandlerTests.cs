using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Auth.AssignRole;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.UnitTests.TestHelpers;

namespace WarehouseManagementSystem.UnitTests.Auth
{
    public class AssignRoleHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IWarehouseRepository> _warehouseRepository = new();
        private readonly AssignRoleCommandValidator _validator = new();

        private AssignRoleHandler CreateHandler(Mock<ICurrentUserService> currentUser) =>
            new(_userRepository.Object, _warehouseRepository.Object, currentUser.Object, _validator);

        [Fact]
        public async Task Handle_Admin_CanAssignAnyRole()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "u@warehouse.com" };
            var warehouse = EntityFactory.CreateWarehouse();

            _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _warehouseRepository.Setup(r => r.GetByIdAsync(warehouse.Id)).ReturnsAsync(warehouse);

            var command = new AssignRoleCommand
            {
                UserId = user.Id,
                Role = "WarehouseManager",
                WarehouseId = warehouse.Id
            };

            await CreateHandler(CurrentUserMockFactory.Admin()).Handle(command);

            Assert.Equal("WarehouseManager", user.Role);
            Assert.Equal(warehouse.Id, user.WarehouseId);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_WarehouseManagerAssigningNonOperatorRole_ThrowsUnauthorizedException()
        {
            var ownWarehouseId = Guid.NewGuid();
            var command = new AssignRoleCommand
            {
                UserId = Guid.NewGuid(),
                Role = "WarehouseManager",
                WarehouseId = ownWarehouseId
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command));

            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WarehouseManagerAssigningOperatorToOtherWarehouse_ThrowsUnauthorizedException()
        {
            var ownWarehouseId = Guid.NewGuid();
            var command = new AssignRoleCommand
            {
                UserId = Guid.NewGuid(),
                Role = "Operator",
                WarehouseId = Guid.NewGuid() // different warehouse
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command));
        }

        [Fact]
        public async Task Handle_WarehouseManagerAssigningOperatorToOwnWarehouse_Succeeds()
        {
            var ownWarehouseId = Guid.NewGuid();
            var user = new User { Id = Guid.NewGuid(), Email = "u@warehouse.com" };

            _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _warehouseRepository.Setup(r => r.GetByIdAsync(ownWarehouseId))
                .ReturnsAsync(EntityFactory.CreateWarehouse(ownWarehouseId));

            var command = new AssignRoleCommand
            {
                UserId = user.Id,
                Role = "Operator",
                WarehouseId = ownWarehouseId
            };

            var handler = CreateHandler(CurrentUserMockFactory.WarehouseManager(ownWarehouseId));
            await handler.Handle(command);

            Assert.Equal("Operator", user.Role);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsNotFoundException()
        {
            _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            var command = new AssignRoleCommand { UserId = Guid.NewGuid(), Role = "Admin" };

            await Assert.ThrowsAsync<NotFoundException>(
                () => CreateHandler(CurrentUserMockFactory.Admin()).Handle(command));
        }

        [Fact]
        public async Task Handle_WarehouseDoesNotExist_ThrowsNotFoundException()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "u@warehouse.com" };
            _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _warehouseRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Warehouse?)null);

            var command = new AssignRoleCommand
            {
                UserId = user.Id,
                Role = "WarehouseManager",
                WarehouseId = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<NotFoundException>(
                () => CreateHandler(CurrentUserMockFactory.Admin()).Handle(command));

            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidRole_ThrowsValidationExceptionBeforeTouchingRepository()
        {
            var command = new AssignRoleCommand { UserId = Guid.NewGuid(), Role = "NotARealRole" };

            await Assert.ThrowsAsync<ValidationException>(
                () => CreateHandler(CurrentUserMockFactory.Admin()).Handle(command));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
