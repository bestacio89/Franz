using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;

namespace Franz.Domain.ValueObjects
{
    public sealed class Author : ValueObject
    {
        public string Value { get; }

        public Author(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Author cannot be empty.", nameof(value));

            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // 👇 Add implicit conversions for EF and dev ergonomics
        public static implicit operator string(Author author) => author.Value;
        public static implicit operator Author(string value) => new Author(value);
    }
}
