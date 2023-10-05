using System.Net.Http.Headers;
using Common.Lang.Extensions;
using Common.LazyTask;
using NodaTime;

namespace Common.OAuth;

using Authenticator = Func<string, Credential, CancellationToken, Task<OAuth2Token>>;

public class OAuthHttpHandler : DelegatingHandler
{
    public static OAuthHttpHandler Build(string apiUrl, string authUrl, Credential credential, Authenticator authenticator)
    {
        return new OAuthHttpHandler(apiUrl, token => Auth(authUrl, credential, authenticator, token));
    }

    private static async LazyTask<OAuth2Token> Auth(string url, Credential credential, Authenticator authenticator, CancellationToken token = default)
    {
        return await authenticator(url, credential, token);
    }

    private readonly string _apiUrl;
    private readonly Func<CancellationToken, LazyTask<OAuth2Token>> _factory;

    private LazyTask<OAuth2Token>? _cache;

    public OAuthHttpHandler(string apiUrl, Func<CancellationToken, LazyTask<OAuth2Token>> factory)
    {
        this._apiUrl = apiUrl;
        this._factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null && request.RequestUri?.ToString().StartsWith(_apiUrl) == true)
        {
            _cache = _cache ?? _factory(cancellationToken);
            var token = await _cache;
            var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
            if (token.Expire < now)
            {
                _cache = _factory(cancellationToken);
                token = await _cache;
            }
            request.Headers.Authorization = new AuthenticationHeaderValue(token.Type, token.Value);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}