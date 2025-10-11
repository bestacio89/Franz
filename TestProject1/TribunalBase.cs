using ArchUnitNET.xUnit;
using System;
using System.Text;
using Xunit;

namespace Franz.Testing
{
  /// <summary>
  /// Shared executor for tribunal-style tests (clean logs, no stack spam).
  /// </summary>
  public abstract class TribunalBase : BaseArchitectureTest
  {
    protected static void ExecuteRule(
      string context,
      string summary,
      Action rule,
      StringBuilder sb,
      Action markViolation)
    {
      try
      {
        rule.Invoke();
      }
      catch (FailedArchRuleException)
      {
        markViolation();
        sb.AppendLine($"🚨 [{context}] {summary}");
      }
      catch (Exception ex)
      {
        markViolation();
        sb.AppendLine($"⚠️ [{context}] Unexpected issue: {ex.Message}");
      }
    }

    protected static void PrintVerdict(StringBuilder sb, int violations, string tribunalName)
    {
      sb.AppendLine();
      sb.AppendLine("═══════════════════════════════════════════════");
      if (violations == 0)
      {
        sb.AppendLine("✅ All laws respected.");
        sb.AppendLine($"🧠 {tribunalName} smiles upon your discipline. The code is pure.");
      }
      else if (violations < 3)
      {
        sb.AppendLine($"⚠️ {violations} minor infraction(s) detected.");
        sb.AppendLine("🧩 Order maintained — but discipline could be tighter.");
      }
      else
      {
        sb.AppendLine($"🔥 {violations} major architectural violation(s) detected!");
        sb.AppendLine("👁 Franz gazes upon your spaghetti... and is not pleased.");
        sb.AppendLine("🗡 Refactor or perish.");
      }
      sb.AppendLine("═══════════════════════════════════════════════");
    }

    protected static void ExecuteTribunal(string tribunalName, Action<StringBuilder, Action> run)
    {
      Console.OutputEncoding = System.Text.Encoding.UTF8;
      var sb = new StringBuilder();
      var violations = 0;
      Action markViolation = () => violations++;

      sb.AppendLine("═══════════════════════════════════════════════");
      sb.AppendLine($" ⚖️  {tribunalName.ToUpper()}");
      sb.AppendLine("═══════════════════════════════════════════════");

      run(sb, markViolation);

      PrintVerdict(sb, violations, tribunalName);
      Console.WriteLine(sb.ToString());

      Assert.True(violations == 0, $"{tribunalName} detected {violations} violation(s). Review the report above.");
    }
  }
}
