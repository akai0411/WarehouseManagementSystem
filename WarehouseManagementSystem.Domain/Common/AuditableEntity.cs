namespace WarehouseManagementSystem.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}