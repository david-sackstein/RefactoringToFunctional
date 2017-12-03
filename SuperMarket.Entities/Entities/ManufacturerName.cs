using FunctionalExtensions;

namespace SuperMarket.Entities
{
    public class ManufacturerName : ValueObject<ManufacturerName>
    {
        public string Value { get; }

        public ManufacturerName(string name)
        {
            Value = name;
        }

        public static Result<ManufacturerName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail<ManufacturerName>("Manufacturer name must not be empty");

            if (name.Length > 256)
                return Result.Fail<ManufacturerName>("Manufacturer name is too long");

            return Result.Ok<ManufacturerName>(new ManufacturerName(name));
        }

        public static explicit operator ManufacturerName(string name)
        {
            return Create(name).Value;
        }

        public static implicit operator string(ManufacturerName productName)
        {
            return productName.Value;
        }

        protected override bool EqualsCore(ManufacturerName other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}