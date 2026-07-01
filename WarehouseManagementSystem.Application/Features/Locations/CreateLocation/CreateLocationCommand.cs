namespace WarehouseManagementSystem.Application.Features.Locations.CreateLocation
{
    public class CreateLocationCommand
    {
        public string Row { get; set; } = string.Empty;
        public int Shelf { get; set; }
        public int Bin { get; set; }
    }
}