using WarehouseManagementSystem.Application.Features.Warehouses.Common;
using WarehouseManagementSystem.Application.Features.Warehouses.CreateWarehouse;
using WarehouseManagementSystem.Domain.Entities;
using WarehouseManagementSystem.Domain.ValueObjects;

namespace WarehouseManagementSystem.Application.Common.Mapping
{
    public static class WarehouseMapper
    {
        public static WarehouseDto ToDto(Warehouse warehouse)
        {
            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Description = warehouse.Description,
                CreatedAt = warehouse.CreatedAt,
                Address = new AddressDto
                {
                    StreetType = warehouse.Address.StreetType,
                    StreetName = warehouse.Address.StreetName,
                    Number = warehouse.Address.Number,
                    PostalCode = warehouse.Address.PostalCode,
                    City = warehouse.Address.City,
                    Country = warehouse.Address.Country,
                    FullAddress = warehouse.Address.ToString()
                }
            };
        }

        public static Warehouse ToEntity(CreateWarehouseCommand command)
        {
            return new Warehouse
            {
                Name = command.Name,
                Description = command.Description,
                Address = new Address(
                    command.StreetType,
                    command.StreetName,
                    command.Number,
                    command.PostalCode,
                    command.City,
                    command.Country)
            };
        }
    }
}