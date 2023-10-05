using System.Text;
using Common.Lang.Extensions;
using Common.Serialization.Json;

namespace Common.Lang.IO;

public static class FileX
{
    /// <summary>
    /// Read all from file at the path. Return null if file not exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static string? ReadAllTextOrNull(string filePath, Encoding? encoding = null, int bufferSize = 4096)
    {
        if (File.Exists(filePath))
            return ReadAllText(filePath, encoding, bufferSize);
        return null;
    }

    /// <summary>
    /// Read all from file at the path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static string ReadAllText(string filePath, Encoding? encoding = null, int bufferSize = 4096)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true);
        using var buffer = new ByteBuffer(fs);
        return buffer.ReadAll(encoding, bufferSize);
    }

    /// <summary>
    /// Read all from file at the path. Return null if file not exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string?> ReadAllTextOrNullAsync(string filePath, Encoding? encoding = null, int bufferSize = 4096, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
            return await ReadAllTextAsync(filePath, encoding, bufferSize, cancellationToken);
        return null;
    }

    /// <summary>
    /// Read all from file at the path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> ReadAllTextAsync(string filePath, Encoding? encoding = null, int bufferSize = 4096, CancellationToken cancellationToken = default)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true);
        using var buffer = new ByteBuffer(fs);
        return await buffer.ReadAllAsync(encoding, bufferSize, cancellationToken);
    }

    /// <summary>
    /// Read file at the path in JSON as type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static T ReadAs<T>(string filePath, Encoding? encoding = null, int bufferSize = 4096)
    {
        return ReadAllText(filePath, encoding, bufferSize).ParseJsonAs<T>();
    }

    /// <summary>
    /// Read file at the path in JSON as type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<T> ReadAsAsync<T>(string filePath, Encoding? encoding = null, int bufferSize = 4096, CancellationToken cancellationToken = default)
    {
        return ReadAllTextAsync(filePath, encoding, bufferSize, cancellationToken).Map(x => x.ParseJsonAs<T>());
    }
}
