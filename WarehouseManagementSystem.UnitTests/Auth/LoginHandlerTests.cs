using FluentValidation;
using Moq;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Auth.Login;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.UnitTests.Auth
{
    public class LoginHandlerTests
    {
        private readonly Mock<IUserRepository> _repository = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly LoginCommandValidator _validator = new();

        private LoginHandler CreateHandler() =>
            new(_repository.Object, _tokenService.Object, _passwordHasher.Object, _validator);

        private static LoginCommand ValidCommand() => new()
        {
            Email = "admin@warehouse.com",
            Password = "Admin123!"
        };

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsToken()
        {
            var command = ValidCommand();
            var user = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "hashed" };

            _repository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _passwordHasher.Setup(h => h.VerifyPassword(user, user.PasswordHash, command.Password))
                .Returns(true);
            _tokenService.Setup(t => t.CreateToken(user)).Returns("fake-jwt-token");

            var result = await CreateHandler().Handle(command);

            Assert.Equal("fake-jwt-token", result.Token);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsUnauthorizedException()
        {
            var command = ValidCommand();
            _repository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() => CreateHandler().Handle(command));

            _tokenService.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
        {
            var command = ValidCommand();
            var user = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "hashed" };

            _repository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
            _passwordHasher.Setup(h => h.VerifyPassword(user, user.PasswordHash, command.Password))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => CreateHandler().Handle(command));

            _tokenService.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationExceptionBeforeTouchingRepository()
        {
            var command = new LoginCommand { Email = "not-an-email", Password = "" };

            await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(command));

            _repository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
