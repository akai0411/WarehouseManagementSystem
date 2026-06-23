namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface ITokenService
    {
        string CreateToken(string email);
    }
}
