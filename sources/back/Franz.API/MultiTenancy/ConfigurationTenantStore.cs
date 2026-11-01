using Franz.Common.MultiTenancy;
using Franz.Common.MultiTenancy.Models;
using Microsoft.Extensions.Configuration;

namespace Franz.API.MultiTenancy;

/// <summary>
/// Configuration-backed tenant registry for the service template.
/// Production deployments can replace ITenantStore with a database-backed
/// implementation without changing tenant resolution or authorization.
/// </summary>
public sealed class ConfigurationTenantStore : ITenantStore
{
    private readonly IReadOnlyDictionary<Guid, TenantInfo> _tenants;
    private readonly IReadOnlyDictionary<string, TenantInfo> _verifiedHosts;

    public ConfigurationTenantStore(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var tenants = new Dictionary<Guid, TenantInfo>();
        var hosts = new Dictionary<string, TenantInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var tenantSection in configuration.GetSection("MultiTenancy:Tenants").GetChildren())
        {
            if (!Guid.TryParse(tenantSection["Id"], out var id) || id == Guid.Empty)
                throw new InvalidOperationException($"Tenant configuration '{tenantSection.Path}' has an invalid Id.");

            var name = tenantSection["Name"];
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Tenant configuration '{tenantSection.Path}' requires a Name.");

            var tenant = new TenantInfo(id, name);
            var status = tenantSection["Status"];
            if (!string.IsNullOrWhiteSpace(status) &&
                !Enum.TryParse(status, ignoreCase: true, out TenantStatus parsedStatus))
                throw new InvalidOperationException($"Tenant '{id}' has an invalid Status.");
            else if (!string.IsNullOrWhiteSpace(status))
                tenant.Status = Enum.Parse<TenantStatus>(status, ignoreCase: true);

            foreach (var domainSection in tenantSection.GetSection("Domains").GetChildren())
            {
                var host = domainSection.Value;
                if (string.IsNullOrWhiteSpace(host))
                    throw new InvalidOperationException($"Tenant '{id}' contains an empty domain.");

                var domain = new TenantDomain(host)
                {
                    TenantId = id,
                    IsVerified = true
                };
                tenant.Domains.Add(domain);

                if (!hosts.TryAdd(domain.Host, tenant))
                    throw new InvalidOperationException($"Verified hostname '{domain.Host}' is assigned to more than one tenant.");
            }

            if (!tenants.TryAdd(id, tenant))
                throw new InvalidOperationException($"Tenant identifier '{id}' is configured more than once.");
        }

        _tenants = tenants;
        _verifiedHosts = hosts;
    }

    public Task<TenantInfo?> FindByIdAsync(Guid id)
        => Task.FromResult(_tenants.GetValueOrDefault(id));

    public Task<TenantInfo?> FindByHostAsync(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return Task.FromResult<TenantInfo?>(null);

        string normalized;
        try
        {
            normalized = TenantDomain.NormalizeHost(host);
        }
        catch (ArgumentException)
        {
            return Task.FromResult<TenantInfo?>(null);
        }

        return Task.FromResult(_verifiedHosts.GetValueOrDefault(normalized));
    }

    public Task<IEnumerable<TenantInfo>> GetAllAsync()
        => Task.FromResult<IEnumerable<TenantInfo>>(_tenants.Values.ToArray());

    public Task<bool> ExistsAsync(Guid id)
        => Task.FromResult(_tenants.ContainsKey(id));
}
