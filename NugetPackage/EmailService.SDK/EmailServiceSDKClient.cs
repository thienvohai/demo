using EmailService.Domain;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EmailService.SDK;

public interface IEmailServiceSDKClient : IDisposable
{
    Task<SendEmailResponse> SendEmailAsync(EmailMessage emailMessage);
    Task<GetEmailDetailResponse> GetEmailDetailAsync(Guid emailId);
    Task<EmailServiceHealthzResponse> HealthzAsync();
}

public class EmailServiceSDKClient : IEmailServiceSDKClient
{
    private bool disposed;
    private EmailServiceSDKClientOptions options { get; }
    private HttpClient client { get; set; } = null;
    private EmailServiceAuthProvider authProvider { get; set; }

    public EmailServiceSDKClient(IOptions<EmailServiceSDKClientOptions> options)
    {
        this.options = options.Value;
        this.client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(100),
        };
        authProvider = new EmailServiceAuthProvider(new EmailServiceAuthOptions()
        {
            AuthEndpoint = this.options.AuthEndpoint,
            ClientId = this.options.ClientId,
            Secret = this.options.ClientSecret,
        });
    }

    public EmailServiceSDKClient(HttpClient httpClient, EmailServiceSDKClientOptions options)
    {
        this.options = options;
        this.client = httpClient;
        authProvider = new EmailServiceAuthProvider(new EmailServiceAuthOptions()
        {
            AuthEndpoint = this.options.AuthEndpoint,
            ClientId = this.options.ClientId,
            Secret = this.options.ClientSecret,
        });
    }

    public async Task<GetEmailDetailResponse> GetEmailDetailAsync(Guid emailId)
    {
        return await ExecuteAsync<GetEmailDetailRequest, GetEmailDetailResponse>(new GetEmailDetailRequest(emailId));
    }

    public async Task<EmailServiceHealthzResponse> HealthzAsync()
    {
        return await ExecuteAsync<EmailServiceHealthzRequest, EmailServiceHealthzResponse>(new EmailServiceHealthzRequest());
    }

    public async Task<SendEmailResponse> SendEmailAsync(EmailMessage emailMessage)
    {
        emailMessage.From = options.From;
        emailMessage.SenderName = options.SenderName;
        var request = new SendEmailRequest(emailMessage);
        return await ExecuteAsync<SendEmailRequest, SendEmailResponse>(request);
    }

    internal async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request)
        where TRequest : EmailServiceRequest
        where TResponse : EmailServiceResponse, new()
    {
        var url = $"{options.EmailServiceEndpoint.TrimEnd('/')}/{request.RequestPath.TrimStart('/')}";
        try
        {
            var tokenInfo = await this.authProvider.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(tokenInfo.AccessToken))
            {
                throw new Exception($"Request Failed because of missing bearer token. URL: {url}. Detail: {tokenInfo.ErrorMessage}");
            }
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken);
            var retryCount = 0;
            var response = new TResponse();
            while (retryCount < 3)
            {
                using (var req = new HttpRequestMessage(){ Method = request.Method, RequestUri = new Uri(url) })
                {
                    if (request.Method != HttpMethod.Get && request.InputParameters != null)
                    {
                        req.Content = new StringContent(
                                    JsonSerializer.Serialize(request.InputParameters),
                                    Encoding.UTF8, "application/json");
                    }
                    using (var result = await this.client.SendAsync(req))
                    {
                        if (await CheckResponseStatus(result, response))
                        {
                            await response.ConvertResponseAsync(result.Content);
                            break;
                        }
                    }
                }
                await Task.Delay(1000);
                retryCount++;
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Request Failed. URL: {url}", ex);
        }
    }

    private async Task<bool> CheckResponseStatus(HttpResponseMessage result, EmailServiceResponse response)
    {
        if (!result.IsSuccessStatusCode)
        {
            response.StatusCode = result.StatusCode;
            string data = await result.Content.ReadAsStringAsync();
            response.IsSuccess = false;
            response.ErrorMessage = data;
            return false;
        }
        else
        {
            response.StatusCode = result.StatusCode;
            response.ErrorMessage = string.Empty;
            return true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                client?.Dispose();
                authProvider?.Dispose();
            }
            disposed = true;
        }
    }
}
