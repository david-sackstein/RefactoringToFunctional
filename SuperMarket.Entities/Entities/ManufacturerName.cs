using System;
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

        public static Result<ManufacturerName> Create(Maybe<string> nameOrNothing)
        {
            return nameOrNothing
                .ToResult("Manufacturer name is invalid")
                .Ensure(n => !string.IsNullOrWhiteSpace(n), "Manufacturer name must not be empty")
                .Ensure(n => n.Length <= 256, "Manufacturer name is too long")
                .OnSuccess(n => new ManufacturerName(n));
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