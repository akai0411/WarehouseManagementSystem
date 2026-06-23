namespace WarehouseManagementSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "User"; // default role

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}