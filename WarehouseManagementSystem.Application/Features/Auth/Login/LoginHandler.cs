using FluentValidation;
using WarehouseManagementSystem.Application.Common.Exceptions;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Application.Features.Auth.Login
{
    public class LoginHandler
    {
        private readonly IUserRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<LoginCommand> _validator;

        public LoginHandler(
            IUserRepository repository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IValidator<LoginCommand> validator)
        {
            _repository = repository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _validator = validator;
        }

        public async Task<LoginResponse> Handle(LoginCommand command)
        {
            var validationResult = await _validator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await _repository.GetByEmailAsync(command.Email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var validPassword = _passwordHasher.VerifyPassword(
                user,
                user.PasswordHash,
                command.Password);

            if (!validPassword)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var token = _tokenService.CreateToken(user);

            return new LoginResponse
            {
                Token = token
            };
        }
    }
}