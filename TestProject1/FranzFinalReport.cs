
using Franz.Testing.ArchitectureTests;
using Franz.Testing.ArchitectureTests.Tribunals;
using System;
using System.Text;
using Xunit;

namespace BookManagement.Testing
{
  /// <summary>
  /// 🏁 Franz Final Report — orchestrates all Tribunals in execution order.
  /// Guarantees consistent logging and unified verdict for the full architecture.
  /// </summary>
  public class FranzFinalReport
  {
    [Fact(DisplayName = "🏁 Franz Final Report — Unified Governance Verdict")]
    public void Execute_All_Tribunals_In_Order()
    {
      Console.OutputEncoding = Encoding.UTF8;
      var sb = new StringBuilder();
      var totalViolations = 0;

      sb.AppendLine("═══════════════════════════════════════════════");
      sb.AppendLine(" 🏛️  FRANZ FINAL REPORT — COMPLETE ARCHITECTURE VERDICT");
      sb.AppendLine("═══════════════════════════════════════════════\n");

      // Sequential Tribunal Order (you can add/remove layers easily)
      totalViolations += RunTribunal<DomainArchitectureTribunal>("🧱 Domain Tribunal", sb);
      totalViolations += RunTribunal<ApplicationArchitectureTribunal>("🧩 Application Tribunal", sb);
      totalViolations += RunTribunal<PersistenceArchitectureTribunal>("🗃 Persistence Tribunal", sb);
      totalViolations += RunTribunal<ApiArchitectureTribunal>("🌐 API Tribunal", sb);
      totalViolations += RunTribunal<ContractsArchitecture>("📜 Contracts Tribunal", sb);

      sb.AppendLine("═══════════════════════════════════════════════");
      sb.AppendLine(" 🧾 FINAL VERDICT");
      sb.AppendLine("═══════════════════════════════════════════════");

      if (totalViolations == 0)
      {
        sb.AppendLine("✅ All Tribunals passed — the architecture stands pure and disciplined.");
        sb.AppendLine("🧠 Franz smiles upon your order and restraint.");
      }
      else if (totalViolations < 5)
      {
        sb.AppendLine($"⚠️ {totalViolations} minor infraction(s) across layers.");
        sb.AppendLine("🧩 The code remains stable, but further refinement is advised.");
      }
      else
      {
        sb.AppendLine($"🔥 {totalViolations} major architectural violation(s) detected!");
        sb.AppendLine("👁 Franz watches with disappointment — discipline must be restored.");
      }

      sb.AppendLine("═══════════════════════════════════════════════");
      Console.WriteLine(sb.ToString());

      Assert.True(totalViolations == 0, $"Final Report: {totalViolations} total violation(s) detected. Review logs above.");
    }

    private int RunTribunal<T>(string name, StringBuilder sb)
    {
      sb.AppendLine($"{name}");
      sb.AppendLine(new string('-', 55));

      try
      {
        // Create instance dynamically
        var tribunal = Activator.CreateInstance(typeof(T));

        // Execute method that represents its main governance check
        var method = typeof(T).GetMethod("Franz_Governance_Tribunal")
                    ?? typeof(T).GetMethod("Application_Governance")
                    ?? typeof(T).GetMethod("Persistence_Governance")
                    ?? typeof(T).GetMethod("Api_Governance")
                    ?? typeof(T).GetMethod("Domain_Governance")
                    ?? typeof(T).GetMethod("Contracts_Governance");

        if (method != null)
        {
          method.Invoke(tribunal, null);
          sb.AppendLine($"✅ {name} passed.\n");
          return 0;
        }

        sb.AppendLine($"🟡 {name} has no defined main governance entry — skipped.\n");
        return 0;
      }
      catch (Exception ex)
      {
        sb.AppendLine($"🚨 {name} failed — {ex.InnerException?.Message ?? ex.Message}\n");
        return 1;
      }
    }
  }
}
