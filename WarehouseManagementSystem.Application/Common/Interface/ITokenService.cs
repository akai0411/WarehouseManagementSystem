using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
