using Common.Lang.Extensions;

namespace Common.Toggle;

public interface IControl
{
    Task<Permissions> GetPermissions(CancellationToken token = default);
    Task<Decision> Decide<Decision>(CancellationToken token = default) where Decision : notnull;
}

public class Control : IControl
{
    private readonly IAuthorizer _authorizer;
    private readonly IDictionary<Type, IPolicy> _policies;

    public Control(IAuthorizer authorizer, IEnumerable<IPolicy> policies)
    {
        _authorizer = authorizer;
        _policies = policies.ToDictionary(x => x.DecisionType);
    }

    public async Task<Permissions> GetPermissions(CancellationToken token = default)
    {
        return await _authorizer.GetPermissions(token);
    }

    public async Task<Decision> Decide<Decision>(CancellationToken token = default) where Decision : notnull
    {
        var factory = _policies.GetOrDefault(typeof(Decision)) ?? throw new ArgumentException($"Missing decision maker for {typeof(Decision)}");
        return await ((IPolicy<Decision>)factory).Create(await GetPermissions(token));
    }
}