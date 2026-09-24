using System.Net.Http.Headers;

namespace AeroRadar.Services;

public class OpenSkyAuthHandler : DelegatingHandler
{
    private readonly GestorTokenOpenSky _gestorToken;

    public OpenSkyAuthHandler(GestorTokenOpenSky gestorToken)
    {
        _gestorToken = gestorToken;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _gestorToken.ObtenerTokenAsync(cancellationToken);

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}