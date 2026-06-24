using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface IPasswordHasher
    {
        string HashPassword(User user, string password);

        bool VerifyPassword(User user, string hashedPassword, string password);
    }
}