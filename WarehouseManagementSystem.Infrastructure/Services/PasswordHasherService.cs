using Microsoft.AspNetCore.Identity;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Services
{
    public class PasswordHasherService : IPasswordHasher
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string hashedPassword, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                hashedPassword,
                password);

            return result == PasswordVerificationResult.Success;
        }
    }
}