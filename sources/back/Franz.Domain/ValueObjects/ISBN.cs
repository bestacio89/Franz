using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Franz.Domain.ValueObjects
{
    public sealed class ISBN : ValueObject
    {
        public string Value { get; }

        public ISBN(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ISBN cannot be empty.", nameof(value));

            // Normalize: keep only digits
            var digits = new string(value.Where(char.IsDigit).ToArray());

            if (digits.Length != 13)
                throw new ArgumentException("ISBN must be exactly 13 digits.", nameof(value));

            Value = digits;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // 👇 Add implicit conversions for EF and ergonomics
        public static implicit operator string(ISBN isbn) => isbn.Value;
        public static implicit operator ISBN(string value) => new ISBN(value);
    }
}
