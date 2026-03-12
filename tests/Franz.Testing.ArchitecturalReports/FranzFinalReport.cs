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

      report.AppendLine("===============================================================");
      report.AppendLine("                FRANZ ARCHITECTURAL SUPREME COURT              ");
      report.AppendLine("===============================================================");
      report.AppendLine($"Session Convened: {DateTime.Now:G}");
      report.AppendLine();

      // Ordered tribunal execution
      results.Add(RunAudit<DomainLayerComplianceAudit>("Domain Layer"));
      results.Add(RunAudit<ApplicationLayerComplianceAudit>("Application Layer"));
      results.Add(RunAudit<PersistenceLayerComplianceAudit>("Persistence Layer"));
      results.Add(RunAudit<ApiLayerComplianceAudit>("API Layer"));
      results.Add(RunAudit<ContractsLayerComplianceAudit>("Contracts Layer"));

      // Summary Section
      report.AppendLine("---------------------------------------------------------------");
      report.AppendLine("                        TRIBUNAL LOGS                          ");
      report.AppendLine("---------------------------------------------------------------");
      report.AppendLine("Layer                    | Result | Duration | Notes");
      report.AppendLine("-------------------------|--------|----------|-----------------");

      foreach (var r in results)
      {
        var result = r.Passed ? "PASS ✅" : "FAIL ❌";
        report.AppendLine($"{r.Tribunal,-24} | {result,-6} | {r.Duration.TotalSeconds,6:F2}s | {r.Message}");
      }

      // --- THE SASS ENGINE ---
      int total = results.Count;
      int failed = results.Count(r => !r.Passed);
      double failureRate = (double)failed / total;

      report.AppendLine("---------------------------------------------------------------");
      report.AppendLine("                      FINAL JUDGMENT                           ");
      report.AppendLine("---------------------------------------------------------------");

      var (verdict, sass) = GetSassLevel(failureRate);

      report.AppendLine($"STATUS: {verdict}");
      report.AppendLine($"DECREE: \"{sass}\"");
      report.AppendLine("===============================================================");

      Console.WriteLine(report.ToString());

      Assert.True(failed == 0, $"The Tribunal has found {failed} layer(s) guilty of architectural treason.");
    }

    private static (string Verdict, string Sass) GetSassLevel(double failureRate)
    {
      return failureRate switch
      {
        0.0 => ("COMPLIANT (GOD-TIER)", "Absolute purity. Uncle Bob just shed a single, joyful tear."),
        <= 0.2 => ("MINOR INFRACTIONS", "Mostly clean, but I found some 'temporary' hacks that look suspiciously permanent."),
        <= 0.5 => ("NEEDS STEROIDS", "This isn't an architecture, it's a suggestion. Fix the bleeding before the Onion dies."),
        <= 0.8 => ("ARCHITECTURAL ANARCHY", "Did you even read the README? The layers are so coupled they're basically married."),
        _ => ("TOTAL DISASTER", "Burn it down. Start over. This code belongs in a museum of 'What Not To Do'.")
      };
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
          return new ReportEntry(title, stopwatch.Elapsed, true, "No audit found.");

        method.Invoke(instance, null);
        stopwatch.Stop();
        return new ReportEntry(title, stopwatch.Elapsed, true, "Compliant");
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        // Extract the real error from the reflection wrapper
        var realError = ex.InnerException?.Message ?? ex.Message;
        return new ReportEntry(title, stopwatch.Elapsed, false, realError);
      }
    }
  }
}