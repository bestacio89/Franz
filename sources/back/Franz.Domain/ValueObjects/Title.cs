using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;

namespace Franz.Domain.ValueObjects
{
    public sealed class Title : ValueObject
    {
        public string Value { get; }

        public Title(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty.", nameof(value));

            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // Implicit conversions for EF and convenience
        public static implicit operator string(Title title) => title.Value;
        public static implicit operator Title(string value) => new Title(value);
    }
}
