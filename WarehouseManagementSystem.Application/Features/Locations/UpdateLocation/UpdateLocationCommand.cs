namespace WarehouseManagementSystem.Application.Features.Locations.UpdateLocation
{
    public class UpdateLocationCommand
    {
        public string Row { get; set; } = string.Empty;
        public int Shelf { get; set; }
        public int Bin { get; set; }
    }
}