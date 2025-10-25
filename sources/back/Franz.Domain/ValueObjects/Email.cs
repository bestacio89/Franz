using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;

namespace Franz.Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(value));

            if (!value.Contains("@"))
                throw new ArgumentException("Email must be valid.", nameof(value));

            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // 👇 Add implicit conversions for EF and convenience
        public static implicit operator string(Email email) => email.Value;
        public static implicit operator Email(string value) => new Email(value);
    }
}
