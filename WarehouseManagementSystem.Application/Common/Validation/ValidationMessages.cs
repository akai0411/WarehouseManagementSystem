namespace WarehouseManagementSystem.Application.Common.Validation
{
    public static class ValidationMessages
    {
        public const string NameRequired = "Product name is required.";
        public const string NameMaxLength = "Product name cannot exceed 100 characters.";

        public const string SkuRequired = "SKU is required.";
        public const string SkuMaxLength = "SKU cannot exceed 50 characters.";

        public const string PriceGreaterThanZero = "Price must be greater than zero.";

        public const string QuantityCannotBeNegative = "Quantity in stock cannot be negative.";

        public const string DescriptionMaxLength = "Description cannot exceed 500 characters.";
    }
}