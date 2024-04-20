using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using Franz.Common.Business.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FranzTesting.TestingConditions
{
  public class CallIAggregateRepositoryMethodCondition : ICondition<IMember>
  {
    public string Description => "Should call method of IAggregateRepository<>";

    public IEnumerable<ConditionResult> Check(IEnumerable<IMember> members, Architecture architecture)
    {
      var failures = new List<ConditionResult>();
      foreach (var member in members.OfType<MethodMember>()) // Ensure it only checks methods
      {
        // Check if the method calls any IAggregateRepository<> method
        bool callsAggregateRepositoryMethod = member.DeclaringType.GetType() == typeof(IAggregateRepository<>);

        if (!callsAggregateRepositoryMethod)
        {
          var message = $"{member.FullName} does not call any IAggregateRepository<> method";
          failures.Add(new ConditionResult(member, false, message));
        }
      }
      return failures;
    }

    public bool CheckEmpty()
    {
      return true; // No members to check should not be considered a failure by default
    }
  }
}
