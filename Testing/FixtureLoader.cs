using System.Dynamic;
using Common.IO;
using Common.Lang.Extensions;

namespace Common.Testing;

public class FixtureLoader : ResourceLoader
{
    public FixtureLoader(string rootDirectory) : base(rootDirectory)
    {
    }

    public async Task<T> LoadAs<T>(string fileName, CancellationToken token = default) where T : class
    {
        var type = typeof(T);

        if (type.IsInterface && !type.IsGenericType)
        {
            var obj = await base.LoadAs<ExpandoObject>(fileName, token: token);
            return obj.MockInterface(type);
        }

        return await base.LoadAs<T>(fileName, token: token);
    }
}