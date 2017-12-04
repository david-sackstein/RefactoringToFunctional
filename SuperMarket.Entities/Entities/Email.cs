using System.Text.RegularExpressions;
using FunctionalExtensions;

namespace SuperMarket.Entities
{
    public class Email : ValueObject<Email>
    {
        public string Value { get; }

        public Email(string name)
        {
            Value = name;
        }

        public static Result<Email> Create(Maybe<string> emailOrNothing)
        {
            if (emailOrNothing.HasNoValue)
                return Result.Fail<Email>("Email is invalid");

            var email = emailOrNothing.Value;

            if (!Regex.IsMatch(email, @"^(.+)@(.+)$"))
                return Result.Fail<Email>("Email is invalid");

            return Result.Ok<Email>(new Email(email));
        }

        public static explicit operator Email(string name)
        {
            return Create(name).Value;
        }

        public static implicit operator string(Email productName)
        {
            return productName.Value;
        }

        protected override bool EqualsCore(Email other)
        {
            return Value == other.Value;
        }

        protected override int GetHashCodeCore()
        {
            return Value.GetHashCode();
        }
    }
}