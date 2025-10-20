using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using Franz.Common.Business.Events;
using Franz.Common.Mediator;
using Franz.Common.Mediator.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Assembly = System.Reflection.Assembly;

namespace Franz.Testing
{
  /// <summary>
  /// Universal architecture context for Franz-based solutions.
  /// Dynamically loads Franz.* (Common, Common.Mediator, Domain, Application, API, Persistence, Contracts).
  /// </summary>
  public abstract class BaseArchitectureTest
  {
    // ─────────────────────────────────────────────
    // Static bootstrap
    // ─────────────────────────────────────────────
    static BaseArchitectureTest()
    {
      Console.OutputEncoding = System.Text.Encoding.UTF8;
      Console.WriteLine("🧩 Preloading solution assemblies...");

      // Include Franz.Common and Franz.Common.Mediator
      const string allowedPattern =
        @".*\.(Common(\.Mediator)?|Mediator|Domain|Application|API|Persistence|Contracts)(\.dll)?$";

      EnsureAssembliesLoaded(allowedPattern);

      // Gather assemblies already loaded into the AppDomain that match our suffixes
      var assembliesToLoad = AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(a => Regex.IsMatch(
            a.GetName().Name,
            @"\.(Common(\.Mediator)?|Mediator|Domain|Application|API|Persistence|Contracts)$",
            RegexOptions.IgnoreCase))
        .Distinct()
        .ToArray();

      // Detect solution prefix (best-effort cosmetic)
      var solutionPrefix = assembliesToLoad
        .Select(a => a.GetName().Name.Split('.')[0])
        .GroupBy(x => x)
        .OrderByDescending(g => g.Count())
        .FirstOrDefault()?.Key ?? "Unknown";

      Console.WriteLine($"⚙️  Detected Solution Prefix: {solutionPrefix}");
      foreach (var asm in assembliesToLoad) Console.WriteLine($"   • {asm.GetName().Name}");

      // Build architecture graph
      BaseArchitecture = new ArchLoader()
        .LoadAssemblies(assembliesToLoad)
        .Build();

      Console.WriteLine("✅ Architecture graph built successfully.\n");

      // Late-binding by suffix (fallback)
      DomainAssembly = TryResolveAssembly(".Domain") ?? assembliesToLoad.FirstOrDefault(a => a.GetName().Name.EndsWith(".Domain", StringComparison.OrdinalIgnoreCase));
      ApplicationAssembly = TryResolveAssembly(".Application") ?? assembliesToLoad.FirstOrDefault(a => a.GetName().Name.EndsWith(".Application", StringComparison.OrdinalIgnoreCase));
      PersistenceAssembly = TryResolveAssembly(".Persistence") ?? assembliesToLoad.FirstOrDefault(a => a.GetName().Name.EndsWith(".Persistence", StringComparison.OrdinalIgnoreCase));
      ApiAssembly = TryResolveAssembly(".API") ?? assembliesToLoad.FirstOrDefault(a => a.GetName().Name.EndsWith(".API", StringComparison.OrdinalIgnoreCase));
      ContractsAssembly = TryResolveAssembly(".Contracts") ?? assembliesToLoad.FirstOrDefault(a => a.GetName().Name.EndsWith(".Contracts", StringComparison.OrdinalIgnoreCase));
      // (Common assemblies are not strictly needed as fields, we just need them loaded into BaseArchitecture)

      // 🔎 Diagnostics — verify key interfaces are visible to the model
      bool scopedExists = BaseArchitecture.Interfaces.Any(i => i.FullName == typeof(Franz.Common.DependencyInjection.IScopedDependency).FullName);
      bool singletonExists = BaseArchitecture.Interfaces.Any(i => i.FullName == typeof(Franz.Common.DependencyInjection.ISingletonDependency).FullName);
      bool icommandExists = BaseArchitecture.Interfaces.Any(i => i.FullName == typeof(ICommand).FullName);
      bool iqueryExists = BaseArchitecture.Interfaces.Any(i => i.FullName == typeof(IQuery<>).FullName);

      if (!scopedExists || !singletonExists || !icommandExists || !iqueryExists)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  Warning: Some Franz.Common.* or Mediator interfaces were NOT found in the architecture graph.");
        if (!scopedExists) Console.WriteLine("   ➜ Missing: Franz.Common.DependencyInjection.IScopedDependency");
        if (!singletonExists) Console.WriteLine("   ➜ Missing: Franz.Common.DependencyInjection.ISingletonDependency");
        if (!icommandExists) Console.WriteLine("   ➜ Missing: Franz.Common.Mediator.Messages.ICommand");
        if (!iqueryExists) Console.WriteLine("   ➜ Missing: Franz.Common.Mediator.Messages.IQuery<>");
        Console.WriteLine("   Hint: Ensure Franz.Common*.dll are copied to the test output (bin) folder.");
        Console.ResetColor();
      }

      // Discover events & handlers (optional cache)
      DomainEventTypes = DomainLayer
        .GetObjects(BaseArchitecture)
        .Where(IsDomainEvent)
        .ToList();

      ApplicationEventHandlerTypes = ApplicationLayer
        .GetObjects(BaseArchitecture)
        .Where(t => t.Name.EndsWith("EventHandler", StringComparison.OrdinalIgnoreCase)
                 || t.Name.EndsWith("NotificationHandler", StringComparison.OrdinalIgnoreCase))
        .ToList();

      HasDomainEvents = DomainEventTypes.Any();
      HasEventHandlers = ApplicationEventHandlerTypes.Any();

      SolutionPrefix = solutionPrefix;
    }

    // ─────────────────────────────────────────────
    // Public static fields used throughout
    // ─────────────────────────────────────────────
    protected static Assembly? DomainAssembly;
    protected static Assembly? ApplicationAssembly;
    protected static Assembly? PersistenceAssembly;
    protected static Assembly? ApiAssembly;
    protected static Assembly? ContractsAssembly;

    protected static readonly Architecture BaseArchitecture;
    protected static string SolutionPrefix = "Unknown";

    // Layers (namespace-based, include nested)
    protected static readonly IObjectProvider<IType> DomainLayer =
      ArchRuleDefinition.Types().That().ResideInNamespaceMatching(@".*\.Domain(\..*)?$").As("Domain Layer");

    protected static readonly IObjectProvider<IType> ApplicationLayer =
      ArchRuleDefinition.Types().That().ResideInNamespaceMatching(@".*\.Application(\..*)?$").As("Application Layer");

    protected static readonly IObjectProvider<IType> PersistenceLayer =
      ArchRuleDefinition.Types().That().ResideInNamespaceMatching(@".*\.Persistence(\..*)?$").As("Persistence Layer");

    protected static readonly IObjectProvider<IType> ApiLayer =
      ArchRuleDefinition.Types().That().ResideInNamespaceMatching(@".*\.API(\..*)?$").As("API Layer");

    protected static readonly IObjectProvider<IType> ContractsLayer =
      ArchRuleDefinition.Types().That().ResideInNamespaceMatching(@".*\.Contracts(\..*)?$").As("Contracts Layer");

    // Discovery caches
    protected static IReadOnlyList<IType> DomainEventTypes = new List<IType>();
    protected static IReadOnlyList<IType> ApplicationEventHandlerTypes = new List<IType>();
    protected static bool HasDomainEvents;
    protected static bool HasEventHandlers;

    // ─────────────────────────────────────────────
    // Helpers (shared)
    // ─────────────────────────────────────────────
    protected static void ReportArchitectureContext()
    {
      Console.WriteLine("═══════════════════════════════════════════════");
      Console.WriteLine($" ⚖️  FRANZ GOVERNANCE — {SolutionPrefix}");
      Console.WriteLine("═══════════════════════════════════════════════");
      Console.WriteLine($"📦 Assemblies:");
      Console.WriteLine($"   • Domain:       {DomainAssembly?.GetName().Name ?? "N/A"}");
      Console.WriteLine($"   • Application:  {ApplicationAssembly?.GetName().Name ?? "N/A"}");
      Console.WriteLine($"   • Persistence:  {PersistenceAssembly?.GetName().Name ?? "N/A"}");
      Console.WriteLine($"   • API:          {ApiAssembly?.GetName().Name ?? "N/A"}");
      Console.WriteLine($"   • Contracts:    {ContractsAssembly?.GetName().Name ?? "N/A"}");
      Console.WriteLine($"🧩 Domain Events:  {DomainEventTypes.Count}");
      Console.WriteLine($"🧩 Handlers:       {ApplicationEventHandlerTypes.Count}");
      Console.WriteLine("═══════════════════════════════════════════════\n");
    }

    protected static bool SkipIfLayerMissing(IObjectProvider<IType> layer, string name)
    {
      var count = layer.GetObjects(BaseArchitecture).Count();
      if (count == 0)
      {
        Console.WriteLine($"🟡 {name} — no types found (virgin template). Skipping.");
        return true;
      }
      return false;
    }

    protected static bool HasAssembly(Assembly? asm, string name)
    {
      if (asm == null)
      {
        Console.WriteLine($"🟡 {name} — assembly not found. Skipping.");
        return false;
      }
      return true;
    }

    protected static void ResetState()
    {
      DomainEventTypes = new List<IType>();
      ApplicationEventHandlerTypes = new List<IType>();
      HasDomainEvents = false;
      HasEventHandlers = false;
      Console.WriteLine("🔄 Architecture state reset.");
    }

    protected static string GetNamespaceSummary()
    {
      var all = BaseArchitecture.Types
        .Select(t => t.Namespace)
        .Where(ns => ns != null)
        .Distinct()
        .OrderBy(ns => ns)
        .Take(10);

      return string.Join(", ", all);
    }

    // ─────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────
    private static Assembly? TryResolveAssembly(string suffix)
    {
      // Try to find an already loaded assembly by suffix
      var asm = AppDomain.CurrentDomain
        .GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

      if (asm != null) return asm;

      // Try to load from disk: *{suffix}.dll
      var file = Directory
        .GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"*{suffix}.dll", SearchOption.AllDirectories)
        .FirstOrDefault();

      if (file != null)
      {
        try
        {
          asm = Assembly.LoadFrom(file);
          Console.WriteLine($"📦 Late-loaded assembly: {asm.GetName().Name}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"⚠️ Could not load {suffix}.dll — {ex.Message}");
        }
      }
      return asm;
    }

    private static void EnsureAssembliesLoaded(string filePattern)
    {
      var basePath = AppDomain.CurrentDomain.BaseDirectory;

      var dlls = Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories)
        .Where(p => Regex.IsMatch(p, filePattern, RegexOptions.IgnoreCase))
        .ToArray();

      foreach (var file in dlls)
      {
        try
        {
          var asmName = AssemblyName.GetAssemblyName(file);
          if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().FullName == asmName.FullName))
          {
            var asm = Assembly.LoadFrom(file);
            Console.WriteLine($"📦 Manually loaded {asm.GetName().Name}");
          }
        }
        catch (BadImageFormatException)
        {
          Console.WriteLine($"🧱 Skipped non-.NET assembly: {Path.GetFileName(file)}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"⚠️ Skipped {Path.GetFileName(file)} — {ex.Message}");
        }
      }
    }

    private static bool IsDomainEvent(IType t)
    {
      if (t is not ArchUnitNET.Domain.Class c) return false;

      var isConcrete = (bool)!c.IsAbstract && !c.IsGeneric;

      var implementsDomainEvent =
        t.ImplementsInterface(BaseArchitecture.Interfaces.FirstOrDefault(i => i.FullName == typeof(IDomainEvent).FullName));

      var implementsIntegrationEvent =
        t.ImplementsInterface(BaseArchitecture.Interfaces.FirstOrDefault(i => i.FullName == typeof(IIntegrationEvent).FullName));

      var isInfraNoise =
           t.FullName.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Persistence", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Repository", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Handler", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Service", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Validator", StringComparison.OrdinalIgnoreCase)
        || t.FullName.Contains("Mongo", StringComparison.OrdinalIgnoreCase);

      return isConcrete && !isInfraNoise && (implementsDomainEvent || implementsIntegrationEvent);
    }
  }
}
