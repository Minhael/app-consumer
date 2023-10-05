namespace Common.Toggle.Privilege;

public interface ICredentialProvider
{
    Task<string[]> GetPrivileges(CancellationToken token = default);
}