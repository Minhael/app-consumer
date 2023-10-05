using Common.Lang.Extensions;

namespace Common.Database;

public class DatabaseFactory : IDatabase
{
    private readonly Task<IDatabase> _task;

    public DatabaseFactory(Task<IDatabase> task)
    {
        _task = task;
    }

    public Task<IDatabase> Build() => _task;

    public Task<IDatabaseSession> OpenSession(CancellationToken cancellationToken = default)
    {
        return _task.Map(x => x.OpenSession(cancellationToken)).Unwrap();
    }
}