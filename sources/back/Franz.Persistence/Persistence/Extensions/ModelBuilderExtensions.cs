using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace Franz.Persistence
{
    public static class ModelBuilderExtensions
    {
        public static PropertyBuilder<TValueObject> PropertyAsValueObject<TEntity, TValueObject>(
            this EntityTypeBuilder<TEntity> builder,
            Func<Domain.Entities.Member, Domain.ValueObjects.Email> value,
            Expression<Func<TEntity, TValueObject>> propertyExpression,
            Func<string, TValueObject> fromString)
            where TEntity : class
            where TValueObject : class
        {
            return builder.Property(propertyExpression)
                .HasConversion(
                    vo => vo!.ToString(),   // to DB
                    value => fromString(value!) // from DB
                );
        }
    }
}
