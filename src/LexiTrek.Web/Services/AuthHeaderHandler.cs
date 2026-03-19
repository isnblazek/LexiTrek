using System.Net.Http.Headers;

namespace LexiTrek.Web.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;

    public AuthHeaderHandler(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
