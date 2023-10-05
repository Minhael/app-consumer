using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Common.Lang.Extensions;

public static class ChannelExtensions
{
#if !NETCOREAPP3_1_OR_GREATER
    public static async IAsyncEnumerable<T> ReadAllAsync<T>(this ChannelReader<T> self, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (self.TryPeek(out _))
        {
            yield return await self.ReadAsync(cancellationToken);
        }
    }
#endif

    public static async Task WriteNextAsync<T>(this ChannelWriter<T> self, T obj, CancellationToken cancellationToken = default)
    {
        while (await self.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
            if (self.TryWrite(obj))
                return;
        throw new ChannelClosedException();
    }

    public static async Task<T> ReadNextAsync<T>(this ChannelReader<T> self, CancellationToken cancellationToken = default)
    {
        while (await self.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            if (self.TryRead(out var item))
                return item;
        throw new ChannelClosedException();
    }
}
