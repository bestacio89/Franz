using ArchUnitNET.xUnit;
using System;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace Franz.Testing.ArchitecturalReports
{
  /// <summary>
  /// Franz Architectural Compliance Engine (Base)
  /// -------------------------------------------------
  /// Provides the standardized execution and reporting framework
  /// for all architecture compliance audits within the Franz suite.
  /// 
  /// Responsibilities:
  ///  • Executes layered audits (tribunals) with consistent formatting.
  ///  • Collects and aggregates rule results.
  ///  • Outputs structured diagnostics for CI/CD pipelines.
  ///  • Normalizes exceptions into unified audit outcomes.
  /// </summary>
  public abstract class ArchitecturalAuditBase : BaseArchitectureTest
  {
    /// <summary>
    /// Executes an architectural tribunal (audit session) and provides
    /// formatted output with severity classification and compliance summary.
    /// </summary>
    protected static void ExecuteTribunal(string tribunalName, Action<StringBuilder, Action> run)
    {
      Console.OutputEncoding = Encoding.UTF8;
      var sb = new StringBuilder();
      int violationCount = 0;
      Action markViolation = () => violationCount++;

      // ─────────────────────────────────────────────
      // HEADER
      // ─────────────────────────────────────────────
      Console.WriteLine();
      Console.WriteLine("===============================================================");
      Console.WriteLine($" ARCHITECTURE COMPLIANCE AUDIT — {tribunalName.ToUpper()}");
      Console.WriteLine("===============================================================");

      try
      {
        run(sb, markViolation);
      }
      catch (Exception ex)
      {
        markViolation();
        sb.AppendLine($"[ERROR] Unhandled exception during tribunal execution: {ex.Message}");
      }

      // ─────────────────────────────────────────────
      // OUTPUT REPORT
      // ─────────────────────────────────────────────
      Console.WriteLine(sb.ToString());

      // ─────────────────────────────────────────────
      // SUMMARY
      // ─────────────────────────────────────────────
      if (violationCount > 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($" RESULT: NON-COMPLIANT — {violationCount} rule violation(s) detected.");
        Console.WriteLine(" ACTION: Review the report and align code with architectural standards.");
        Console.WriteLine("---------------------------------------------------------------");
        Console.ResetColor();

        Assert.Fail($"{tribunalName} detected {violationCount} architectural violation(s).");
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine(" RESULT: COMPLIANT — No architectural violations detected.");
        Console.WriteLine(" STATUS: System conforms to defined architecture guidelines.");
        Console.WriteLine("---------------------------------------------------------------");
        Console.ResetColor();
      }

      Console.WriteLine(); // Final spacing for readability
    }

    /// <summary>
    /// Executes an individual rule within a tribunal, providing detailed
    /// result classification (Pass, Skip, Violation, or Error).
    /// </summary>
    protected static void ExecuteRule(
      string context,
      string summary,
      Action rule,
      StringBuilder sb,
      Action markViolation)
    {
      try
      {
        rule();
        sb.AppendLine($"[PASS] {context} — {summary}");
      }
      catch (FailedArchRuleException ex)
      {
        if (ex.Message.Contains("requires positive evaluation", StringComparison.OrdinalIgnoreCase))
        {
          sb.AppendLine($"[SKIP] {context} — {summary} (no applicable targets)");
        }
        else if (ex.Message.Contains("no classes", StringComparison.OrdinalIgnoreCase) ||
                 ex.Message.Contains("no objects", StringComparison.OrdinalIgnoreCase))
        {
          sb.AppendLine($"[SKIP] {context} — {summary} (no applicable objects)");
        }
        else
        {
          markViolation();
          sb.AppendLine($"[FAIL] {context} — {summary}");
        }
      }
      catch (XunitException ex)
      {
        markViolation();
        sb.AppendLine($"[FAIL] {context} — {summary} (assertion failure: {ex.Message})");
      }
      catch (Exception ex)
      {
        markViolation();
        sb.AppendLine($"[ERROR] {context} — Unexpected error: {ex.Message}");
      }
    }
  }
}
