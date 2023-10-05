namespace Common.Toggle;

public interface IAuthorizer
{
    Task<Permissions> GetPermissions(CancellationToken token = default);
}