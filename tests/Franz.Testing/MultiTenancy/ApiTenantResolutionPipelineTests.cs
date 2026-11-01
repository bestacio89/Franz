using System.Security.Claims;
using Franz.API.MultiTenancy;
using Franz.Common.MultiTenancy.Models;
using Franz.Common.MultiTenancy.TenantResolution;

namespace Franz.Testing.MultiTenancy;

public sealed class ApiTenantResolutionPipelineTests
{
    [Fact]
    public async Task ResolveAsync_WhenResolversAgree_ReturnsResolvedTenant()
    {
        var tenant = new TenantInfo(Guid.NewGuid(), "tenant-a");
        var pipeline = new ApiTenantResolutionPipeline(
        [
            new StubResolver(100, TenantResolutionResult.Success(tenant, TenantResolutionSource.Header)),
            new StubResolver(200, TenantResolutionResult.Success(tenant, TenantResolutionSource.JwtClaim))
        ]);

        var result = await pipeline.ResolveAsync();

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal(tenant.Id, result.Tenant!.Id);
    }

    [Fact]
    public async Task ResolveAsync_WhenResolversDisagree_RejectsRequest()
    {
        var headerTenant = new TenantInfo(Guid.NewGuid(), "tenant-a");
        var claimTenant = new TenantInfo(Guid.NewGuid(), "tenant-b");
        var pipeline = new ApiTenantResolutionPipeline(
        [
            new StubResolver(100, TenantResolutionResult.Success(headerTenant, TenantResolutionSource.Header)),
            new StubResolver(200, TenantResolutionResult.Success(claimTenant, TenantResolutionSource.JwtClaim))
        ]);

        var result = await pipeline.ResolveAsync();

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains("Conflicting tenant identities", result.Message);
    }

    [Fact]
    public async Task ResolveAsync_WhenSuppliedResolverFails_DoesNotFallBackToAnotherIdentity()
    {
        var tenant = new TenantInfo(Guid.NewGuid(), "tenant-a");
        var pipeline = new ApiTenantResolutionPipeline(
        [
            new StubResolver(50, TenantResolutionResult.FailedResult(TenantResolutionSource.Host, "Unknown host")),
            new StubResolver(100, TenantResolutionResult.Success(tenant, TenantResolutionSource.Header))
        ]);

        var result = await pipeline.ResolveAsync();

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Equal(TenantResolutionSource.Host, result.Source);
    }

    private sealed class StubResolver(int order, TenantResolutionResult? result) : ITenantResolver
    {
        public int Order => order;

        public Task<TenantResolutionResult?> ResolveTenantAsync(object? context)
            => Task.FromResult(result);
    }
}

public sealed class ClaimsTenantRequestAuthorizerTests
{
    [Fact]
    public async Task IsAuthorizedAsync_WhenMembershipClaimContainsTenant_ReturnsTrue()
    {
        var tenant = new TenantInfo(Guid.NewGuid(), "tenant-a");
        var principal = CreatePrincipal("tenant_ids", tenant.Id.ToString());
        var authorizer = new ClaimsTenantRequestAuthorizer("tenant_ids");

        Assert.True(await authorizer.IsAuthorizedAsync(principal, tenant));
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenOnlyResolutionClaimExists_ReturnsFalse()
    {
        var tenant = new TenantInfo(Guid.NewGuid(), "tenant-a");
        var principal = CreatePrincipal("tenant_id", tenant.Id.ToString());
        var authorizer = new ClaimsTenantRequestAuthorizer("tenant_ids");

        Assert.False(await authorizer.IsAuthorizedAsync(principal, tenant));
    }

    private static ClaimsPrincipal CreatePrincipal(string claimType, string value)
        => new(new ClaimsIdentity([new Claim(claimType, value)], authenticationType: "test"));
}
