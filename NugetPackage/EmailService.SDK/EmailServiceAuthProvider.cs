using IdentityModel;
using IdentityModel.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace EmailService.SDK;

public class EmailServiceAuthOptions
{
    public string AuthEndpoint { get; set; }
    public string ClientId { get; set; }
    public string Secret { get; set; }
}

public class EmailServiceAuthProvider : IDisposable
{
    private bool disposed;
    private EmailServiceAuthOptions settings { get; set; }
    private HttpClient client { get; set; }

    public EmailServiceAuthProvider(EmailServiceAuthOptions settings)
    {
        this.settings = settings;
        this.client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(100),
        };
    }

    private static ConcurrentDictionary<string, TokenInfo> CachedTokenInfo = new ConcurrentDictionary<string, TokenInfo>();
    private const string DefaultTokenCacheKey = "EmailServiceToken";

    public async Task<TokenInfo> GetAccessTokenAsync(string tokenCacheKeyName = "")
    {
        var cachedTokenKey = string.IsNullOrEmpty(tokenCacheKeyName) ? DefaultTokenCacheKey : tokenCacheKeyName;
        var hasCachedToken = CachedTokenInfo.TryGetValue(cachedTokenKey, out var accessToken);
        if (hasCachedToken && accessToken != null && !string.IsNullOrEmpty(accessToken.AccessToken))
        {
            accessToken = ValidateLifetime(accessToken.AccessToken);
            if (accessToken.IsValid)
                return accessToken;
        }

        accessToken = await RequestTokenInfoAsync();
        CachedTokenInfo[cachedTokenKey] = accessToken;
        return accessToken;
    }

    private async Task<TokenInfo> RequestTokenInfoAsync()
    {
        try
        {
            var disco = await this.client.GetDiscoveryDocumentAsync(settings.AuthEndpoint);
            if (disco.IsError)
            {
                throw new Exception("Email Serivce Authentication Provider Error: Get discovery documents failed. ", disco.Exception);
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = settings.ClientId,
                ClientSecret = settings.Secret,
            });
            if (tokenResponse.IsError)
            {
                throw new Exception("Email Serivce Authentication Provider Error: Get client credentials token failed. ", tokenResponse.Exception);
            }

            return ValidateLifetime(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            return new TokenInfo()
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public TokenInfo ValidateLifetime(string accessToken)
    {
        var token = new TokenInfo()
        {
            IsValid = true,
            AccessToken = accessToken
        };
        var parts = accessToken.Split('.');
        var claims = parts[1];
        var claimsJson = Encoding.UTF8.GetString(Base64Url.Decode(claims));
        var jsonDocument = JsonDocument.Parse(claimsJson);
        var rootElement = jsonDocument.RootElement;
        var hasFromValue = rootElement.TryGetProperty("nbf", out var fromElement);
        var hasToValue = rootElement.TryGetProperty("exp", out var toElement);
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var padding = new TimeSpan(0, 1, 0);
        DateTime now = DateTime.UtcNow;

        if (!hasFromValue || !hasToValue)
            return new TokenInfo()
            {
                IsValid = false,
                ErrorMessage = "Email Serivce Authentication Provider Error: Token missed datetime valid info"
            };

        var validFrom = unixEpoch.AddSeconds(fromElement.GetInt64());
        if (now < validFrom - padding)
        {
            token.IsValid = false;
            token.ErrorMessage = $"LifetimeError, The token is not valid until {validFrom}.";
        }
        var validTo = unixEpoch.AddSeconds(toElement.GetInt64());
        if (now > validTo - padding)
        {
            token.IsValid = false;
            token.ErrorMessage += "\n" + $"LifetimeError, The token is not valid after {validTo}.";
        }
        return token;
    }

    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
    }

    private void dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                client?.Dispose();
            }
            disposed = true;
        }
    }
}

public class TokenInfo
{
    public string AccessToken { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

