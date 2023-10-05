using Common.Lang.Extensions;

namespace Common.Toggle.Privilege;

public class PrivilegeAuthorizer : IAuthorizer
{
    private readonly IEnumerable<ICredentialProvider> _providers;
    private readonly IEnumerable<IPermissionResolver> _resolvers;

    public PrivilegeAuthorizer(IEnumerable<ICredentialProvider> providers, IEnumerable<IPermissionResolver> resolvers)
    {
        _providers = providers;
        _resolvers = resolvers;
    }

    public async Task<Permissions> GetPermissions(CancellationToken token = default)
    {
        var raw = await Task.WhenAll(_providers.Select(x => x.GetPrivileges(token)));
        var privileges = raw.Flatten().Distinct().ToArray();
        var resolved = await Task.WhenAll(_resolvers.Select(x => x.Resolve(privileges, token)));
        return new Permissions(resolved.Aggregate((acc, x) => acc.Merge(x)));
    }
}