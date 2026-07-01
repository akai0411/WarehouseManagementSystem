namespace WarehouseManagementSystem.Domain.ValueObjects
{
    public class Address
    {
        public string StreetType { get; private set; }
        public string StreetName { get; private set; }
        public string Number { get; private set; }
        public string PostalCode { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }

        private Address() { } // Required by EF Core

        public Address(
            string streetType,
            string streetName,
            string number,
            string postalCode,
            string city,
            string country)
        {
            StreetType = streetType;
            StreetName = streetName;
            Number = number;
            PostalCode = postalCode;
            City = city;
            Country = country;
        }

        public override string ToString()
        {
            return $"{StreetType} {StreetName} {Number}, {PostalCode} {City}, {Country}";
        }
    }
}