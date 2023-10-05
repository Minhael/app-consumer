using System.Net.Http.Headers;
using System.Text;
using Common.Lang.Extensions;
using NodaTime;

namespace Common.OAuth;

public static class IdentityServer3
{
    public record UserInfo(string Username, string SubgroupCode, bool IsDevRole, string OrginalUsername, string OrginalSubgroup);
    private sealed record AuthResp(string token_type, string access_token, long expires_in);

    public static async Task<OAuth2Token> Authenticate(string url, Credential credential, CancellationToken token = default)
    {
        //  https://identityserver.github.io/Documentation/docsv2/endpoints/token.html
        using var client = new HttpClient();
        var content = new
        {
            grant_type = "client_credentials",
            scope = credential.Scope,
            client_id = credential.ClientId,
            client_secret = credential.Secret
        };
        var payload = content.GetType().GetProperties().Select(f => $"{f.Name}={f.GetValue(content, null)}").ToArray().Let(a => string.Join("&", a));
        var body = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");
        var now = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        var response = await client.PostWithRetryAsync($"{url}/connect/token", body, cancellationToken: token);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<AuthResp>(token: token);

        return new OAuth2Token(result.token_type, result.access_token, now + result.expires_in * 1000/*s*/);
    }

    public static async Task<UserInfo> GetUserInfo(string url, string credential, CancellationToken token = default)
    {
        //  https://identityserver.github.io/Documentation/docsv2/endpoints/userinfo.html
        using var client = new HttpClient();
        var (type, value, _) = credential.Split(' ');
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/connect/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue(type, value);
        var response = await client.SendWithRetryAsync(request, cancellationToken: token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<UserInfo>(token: token);
    }
}