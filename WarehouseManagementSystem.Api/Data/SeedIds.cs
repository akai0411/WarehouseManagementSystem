namespace WarehouseManagementSystem.Api.Data
{
    public static class SeedIds
    {
        // Warehouses
        public static readonly Guid MadridWarehouseId =
            Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        public static readonly Guid BarcelonaWarehouseId =
            Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        // Madrid Zones
        public static readonly Guid MadridZoneElectronicsId =
            Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        public static readonly Guid MadridZoneRefrigeratedId =
            Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123");
        public static readonly Guid MadridZoneHighValueId =
            Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234");

        // Barcelona Zones
        public static readonly Guid BarcelonaZoneElectronicsId =
            Guid.Parse("f6a7b8c9-d0e1-2345-fabc-456789012345");
        public static readonly Guid BarcelonaZoneRefrigeratedId =
            Guid.Parse("a7b8c9d0-e1f2-3456-abcd-567890123456");
        public static readonly Guid BarcelonaZoneHighValueId =
            Guid.Parse("b8c9d0e1-f2a3-4567-bcde-678901234567");

        // Products
        public static readonly Guid LaptopProductId =
            Guid.Parse("c9d0e1f2-a3b4-5678-cdef-789012345678");
    }
}