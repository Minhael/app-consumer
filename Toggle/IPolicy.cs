namespace Common.Toggle;

public interface IPolicy
{
    Type DecisionType { get; }
    Task<object> Create(Permissions permissions, CancellationToken token = default);
}

public interface IPolicy<Decision> : IPolicy where Decision : notnull
{
    new Task<Decision> Create(Permissions permissions, CancellationToken token = default);
}

public abstract class Policy<Decision> : IPolicy<Decision> where Decision : notnull
{
    public abstract Task<Decision> Create(Permissions permissions, CancellationToken token = default);

    public Type DecisionType => typeof(Decision);

    async Task<object> IPolicy.Create(Permissions permissions, CancellationToken token) => await Create(permissions, token);
}
