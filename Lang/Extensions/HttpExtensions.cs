using System.Net;
using System.Net.Sockets;
using Common.Serialization.Json;
using Polly;

namespace Common.Lang.Extensions;

public static class HttpExtensions
{
    public static Task<HttpResponseMessage> GetWithRetryAsync(this HttpClient client, string requestUri, int retryCount = 3, long maxPeriodMs = 30000, PolicyBuilder<HttpResponseMessage>? fallbackConditions = null, CancellationToken cancellationToken = default)
    {
        return ExchangeAsync(requestUri, retryCount, maxPeriodMs, (uri, token) => client.GetAsync(uri, token), fallbackConditions, cancellationToken);
    }

    public static Task<HttpResponseMessage> PostWithRetryAsync(this HttpClient client, string requestUri, HttpContent content, int retryCount = 3, long maxPeriodMs = 30000, PolicyBuilder<HttpResponseMessage>? fallbackConditions = null, CancellationToken cancellationToken = default)
    {
        return ExchangeAsync(requestUri, retryCount, maxPeriodMs, (uri, token) => client.PostAsync(uri, content, token), fallbackConditions, cancellationToken);
    }

    public static Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient client, HttpRequestMessage request, int retryCount = 3, long maxPeriodMs = 30000, PolicyBuilder<HttpResponseMessage>? fallbackConditions = null, CancellationToken cancellationToken = default)
    {
        return ExchangeAsync(request.RequestUri!.ToString(), retryCount, maxPeriodMs, (uri, token) => client.SendAsync(request.Also(x => x.RequestUri = new Uri(uri)), token), fallbackConditions, cancellationToken);
    }

    private static Task<HttpResponseMessage> ExchangeAsync(string requestUri, int retryCount, long maxPeriodMs, Func<string, CancellationToken, Task<HttpResponseMessage>> onExchange, PolicyBuilder<HttpResponseMessage>? fallbackConditions = null, CancellationToken cancellationToken = default)
    {
        if (fallbackConditions == null)
            fallbackConditions = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
                                       .OrInner<SocketException>();

        var hosts = requestUri.Split(';').Where(x => !x.IsNullOrWhiteSpace()).Select(x => x.Trim()).ToArray();

        var retry = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
                          .OrInner<SocketException>()
                          .WaitAndRetryAsync(
                              retryCount,
                              i => TimeSpan.FromMilliseconds(Math.Min(maxPeriodMs, 1000L * Math.Pow(2, i - 1)))
                          );

        return hosts.Skip(1)
                    .Select(x => fallbackConditions.FallbackAsync((token) => retry.ExecuteAsync(() => onExchange(x, token))))
                    .Reverse()
                    .WithType<IAsyncPolicy<HttpResponseMessage>>()
                    .Append(retry)
                    .ToArray()
                    .Let(x => x.Length switch
                    {
                        0 => throw new ArgumentException("Invalid uri"),
                        1 => x[0],
                        _ => Policy.WrapAsync(x)
                    })
                    .ExecuteAsync(() => onExchange(hosts[0], cancellationToken));
    }

#if !NET5_0_OR_GREATER
    public static async Task<T> ReadAsAsync<T>(this HttpContent self, IJsonizer? jsonizer = null, CancellationToken token = default)
    {
        using var iss = await self.ReadAsStreamAsync();
        return await iss.ParseJsonAs<T>(jsonizer, token);
    }
#endif

#if NET5_0_OR_GREATER
    public static async Task<T> ReadAsAsync<T>(this HttpContent self, IJsonizer? jsonizer = null, CancellationToken token = default)
    {
        using var iss = await self.ReadAsStreamAsync(token);
        return await iss.ParseJsonAs<T>(jsonizer, token);
    }
#endif
}