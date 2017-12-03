using FunctionalExtensions;

namespace SuperMarket.Entities
{
    public class ProductName : ValueObject<ProductName>
    {
        public string Value { get; }

        public ProductName(string name)
        {
            Value = name;
        }

        public static Result<ProductName> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail<ProductName>("Product name must not be empty");

            if (name.Length > 100)
                return Result.Fail<ProductName>("Product name is too long");

            return Result.Ok(new ProductName(name));
        }

        public static explicit operator ProductName(string name)
        {
            return Create(name).Value;
        }

        public static implicit operator string(ProductName productName)
        {
            return productName.Value;
        }

        protected override bool EqualsCore(ProductName other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}