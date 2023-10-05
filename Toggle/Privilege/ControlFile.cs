using Common.IO;
using Common.Lang.Extensions;

namespace Common.Toggle.Privilege;

public class ControlFile : IPermissionResolver
{
    private readonly ResourceLoader _loader;

    public ControlFile(ResourceLoader loader)
    {
        _loader = loader;
    }

    public async Task<IDictionary<string, string?>> Resolve(string[] privileges, CancellationToken token = default)
    {
        IDictionary<string, string?> result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var set in privileges)
        {
            var layer = await _loader.LoadAsOrNull<Dictionary<string, string?>>($"{set}.json", token: token);
            result = result.Merge(layer);
        }
        return result;
    }
}