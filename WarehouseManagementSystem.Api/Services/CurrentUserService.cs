using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WarehouseManagementSystem.Application.Common.Interface;

namespace WarehouseManagementSystem.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId => Guid.Parse(
            _httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

        public string Role =>
            _httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role)!;

        public Guid? WarehouseId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext!.User
                    .FindFirstValue("WarehouseId");
                return value != null ? Guid.Parse(value) : null;
            }
        }

        public string? Country =>
            _httpContextAccessor.HttpContext!.User
                .FindFirstValue("Country");

        public bool IsAdmin => Role == "Admin";
        public bool IsWarehouseManager => Role == "WarehouseManager";
        public bool IsRegionalManager => Role == "RegionalManager";
        public bool IsOperator => Role == "Operator";
    }
}