using AeroRadar.Configuration;
using Microsoft.Extensions.Options;

namespace AeroRadar.Services;

public class GestorTokenOpenSky
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenSkyOptions _opciones;
    private readonly ILogger<GestorTokenOpenSky> _logger;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    private string? _token;
    private DateTimeOffset _caducaUtc;

    public GestorTokenOpenSky(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenSkyOptions> opciones,
        ILogger<GestorTokenOpenSky> logger)
    {
        _httpClientFactory = httpClientFactory;
        _opciones = opciones.Value;
        _logger = logger;
    }

    public async Task<string?> ObtenerTokenAsync(CancellationToken cancellationToken)
    {
        if (!_opciones.TieneCredenciales)
        {
            return null;
        }

        await _semaforo.WaitAsync(cancellationToken);
        try
        {
            if (_token is not null && DateTimeOffset.UtcNow < _caducaUtc)
            {
                return _token;
            }

            var cliente = _httpClientFactory.CreateClient();
            var contenido = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _opciones.ClientId!,
                ["client_secret"] = _opciones.ClientSecret!
            });

            var respuesta = await cliente.PostAsync(_opciones.UrlToken, contenido, cancellationToken);
            respuesta.EnsureSuccessStatusCode();

            var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaToken>(cancellationToken)
                ?? throw new InvalidOperationException("OpenSky ha devuelto una respuesta de token vacía");

            _token = datos.AccessToken;
            _caducaUtc = DateTimeOffset.UtcNow.AddSeconds(datos.ExpiraEnSegundos - 60);

            _logger.LogInformation("Nuevo token de OpenSky obtenido, válido hasta {Caducidad}", _caducaUtc);
            return _token;
        }
        finally
        {
            _semaforo.Release();
        }
    }
}