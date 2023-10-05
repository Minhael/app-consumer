using System.Text;
using Common.Lang.Extensions;

namespace Common.Lang.IO;

/// <summary>
/// Buffer to read stream reactively.
/// </summary>
public sealed class ByteBuffer : IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly MemoryStream _oss = new();

    public ByteBuffer(Stream stream)
    {
        _stream = stream;
    }

    public void Dispose()
    {
        _oss.Dispose();
        _stream.Close();
    }

    public async ValueTask DisposeAsync()
    {
#if NETCOREAPP3_1_OR_GREATER
        await _oss.DisposeAsync();
        await _stream.DisposeAsync();
#else
            _oss.Dispose();
            _stream.Dispose();
            await Task.CompletedTask;
#endif
    }

    /// <summary>
    /// Read a line from the stream.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="EndOfStreamException"></exception>
    public async Task<string> ReadlnAsync(Encoding? encoding = null, int bufferSize = 2048, CancellationToken token = default)
    {
        var buffer = new byte[bufferSize];
        var lf = Convert.ToByte('\n');

        long index;
        while (!token.IsCancellationRequested)
        {
            index = _oss.GetBuffer().IndexOf(lf, (int)_oss.Position, (int)_oss.Length);
            switch (index)
            {
                case -1:
                    var received = await _stream.ReadAsync(buffer, 0, bufferSize, token);
                    await WriteBuffer(buffer, 0, received, token);
                    if (received < 1) throw new EndOfStreamException();
                    break;
                default:
                    return await ReadString(index + 1 - _oss.Position, encoding, token);
            }
        }

        throw new EndOfStreamException();
    }

    /// <summary>
    /// Read all from stream.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public string ReadAll(Encoding? encoding = null, int bufferSize = 2048)
    {
        var buffer = new byte[bufferSize];
        _oss.Compact();

        while (true)
        {
            var received = _stream.Read(buffer, 0, bufferSize);

            if (received < 1)
                break;

            _oss.Write(buffer, 0, received);
        }

        _oss.Flush();
        _oss.Flip();
        return ReadString(_oss.Length, encoding);
    }

    /// <summary>
    /// Read all from stream.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="bufferSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<string> ReadAllAsync(Encoding? encoding = null, int bufferSize = 2048, CancellationToken token = default)
    {
        var buffer = new byte[bufferSize];
        _oss.Compact();

        while (!token.IsCancellationRequested)
        {
            var received = await _stream.ReadAsync(buffer, 0, bufferSize, token);

            if (received < 1)
                break;

            await _oss.WriteAsync(buffer, 0, received, token);
        }

        await _oss.FlushAsync(token);
        _oss.Flip();
        return await ReadString(_oss.Length, encoding, token);
    }

    private async Task WriteBuffer(byte[] buffer, int offset, int length, CancellationToken token)
    {
        _oss.Compact();
        await _oss.WriteAsync(buffer, offset, length, token);
        await _oss.FlushAsync(token);
        _oss.Flip();
    }

    private string ReadString(long length, Encoding? encoding)
    {
        var output = new byte[length];
        var count = _oss.Read(output, 0, (int)length);
        return output.Decode(0, count, encoding);
    }

    private async Task<string> ReadString(long length, Encoding? encoding, CancellationToken token)
    {
        var output = new byte[length];
        var count = await _oss.ReadAsync(output, 0, (int)length, token);
        return output.Decode(0, count, encoding);
    }
}