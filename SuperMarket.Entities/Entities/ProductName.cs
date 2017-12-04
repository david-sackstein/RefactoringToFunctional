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

        public static Result<ProductName> Create(Maybe<string> nameOrNothing)
        {
            return nameOrNothing
                .ToResult("Product name is invalid")
                .Ensure(n => !string.IsNullOrWhiteSpace(n), "Product name must not be empty")
                .Ensure(n => n.Length <= 100, "Product name is too long")
                .OnSuccess(n => new ProductName(n));
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