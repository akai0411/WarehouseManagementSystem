using WarehouseManagementSystem.Application.Features.Auth.Register;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class UserMapper
    {
        public static User ToEntity(RegisterCommand command)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email
            };
        }
    }
}