namespace WarehouseManagementSystem.Application.Common.Interface
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string Role { get; }
        Guid? WarehouseId { get; }
        string? Country { get; }
        bool IsAdmin { get; }
        bool IsWarehouseManager { get; }
        bool IsRegionalManager { get; }
        bool IsOperator { get; }
    }
}