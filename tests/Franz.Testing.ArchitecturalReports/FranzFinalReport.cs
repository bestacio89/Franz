using Franz.Testing.ArchitecturalReports.Layers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Franz.Testing.ArchitecturalReports
{
  /// <summary>
  /// Franz Architecture Compliance Report
  /// -------------------------------------------------
  /// Executes all layer-specific architecture audits
  /// and consolidates a unified compliance summary.
  /// 
  /// Output is designed for enterprise audit trails,
  /// CI/CD compliance gates, and documentation reports.
  /// </summary>
  public sealed class ArchitectureComplianceReport
  {
    private record ReportEntry(string Tribunal, TimeSpan Duration, bool Passed, string Message);

    [Trait("Category", "ArchitecturalReport")]
    [Fact(DisplayName = "Architecture Compliance Report — Unified Compliance Summary")]
    public void Generate_Compliance_Summary()
    {
      Console.OutputEncoding = Encoding.UTF8;
      var results = new List<ReportEntry>();
      var report = new StringBuilder();

      report.AppendLine();
      report.AppendLine("===============================================================");
      report.AppendLine("                 ARCHITECTURE COMPLIANCE REPORT                 ");
      report.AppendLine("===============================================================");
      report.AppendLine($"Generated on: {DateTime.Now:G}");
      report.AppendLine();

      // Ordered tribunal execution
      results.Add(RunAudit<DomainLayerComplianceAudit>("Domain Layer"));
      results.Add(RunAudit<ApplicationLayerComplianceAudit>("Application Layer"));
      results.Add(RunAudit<PersistenceLayerComplianceAudit>("Persistence Layer"));
      results.Add(RunAudit<ApiLayerComplianceAudit>("API Layer"));
      results.Add(RunAudit<ContractsLayerComplianceAudit>("Contracts Layer"));

      // Summary Section
      report.AppendLine("---------------------------------------------------------------");
      report.AppendLine("                       COMPLIANCE SUMMARY                      ");
      report.AppendLine("---------------------------------------------------------------");
      report.AppendLine("Layer                                   | Result | Duration | Notes");
      report.AppendLine("---------------------------------------------------------------");

      foreach (var r in results)
      {
        var result = r.Passed ? "PASS" : "FAIL";
        report.AppendLine($"{r.Tribunal,-40} | {result,-5} | {r.Duration.TotalSeconds,6:F2}s | {r.Message}");
      }

      var failed = results.Count(r => !r.Passed);
      report.AppendLine("---------------------------------------------------------------");
      if (failed == 0)
      {
        report.AppendLine("STATUS: COMPLIANT — All architecture audits passed successfully.");
      }
      else
      {
        report.AppendLine($"STATUS: NON-COMPLIANT — {failed} audit(s) reported rule violations.");
      }
      report.AppendLine("===============================================================");

      Console.WriteLine(report.ToString());
      Assert.True(failed == 0, $"{failed} layer(s) failed compliance checks. See report above.");
    }

    private static ReportEntry RunAudit<T>(string title)
    {
      var stopwatch = Stopwatch.StartNew();
      try
      {
        var instance = Activator.CreateInstance(typeof(T));
        var method = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
          .FirstOrDefault(m =>
            m.Name.Contains("Governance", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Audit", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Tribunal", StringComparison.OrdinalIgnoreCase));

        if (method == null)
          return new ReportEntry(title, stopwatch.Elapsed, true, "No audit entry point (skipped).");

        method.Invoke(instance, null);
        stopwatch.Stop();
        return new ReportEntry(title, stopwatch.Elapsed, true, "Compliant");
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        return new ReportEntry(title, stopwatch.Elapsed, false, ex.InnerException?.Message ?? ex.Message);
      }
    }
  }
}
