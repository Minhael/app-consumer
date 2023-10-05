namespace Common.Toggle.Privilege;

public interface IPermissionResolver
{
    Task<IDictionary<string, string?>> Resolve(string[] privileges, CancellationToken token = default);
}