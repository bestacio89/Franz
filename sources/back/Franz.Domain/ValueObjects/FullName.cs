using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;

namespace Franz.Domain.ValueObjects
{
    public sealed class FullName : ValueObject
    {
        public string Value { get; }

        public FullName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Full name cannot be empty.", nameof(value));

            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // 👇 Add implicit conversions for EF and convenience
        public static implicit operator string(FullName name) => name.Value;
        public static implicit operator FullName(string value) => new FullName(value);
    }
}
