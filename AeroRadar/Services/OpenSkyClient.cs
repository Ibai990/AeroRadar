using AeroRadar.Configuration;
using AeroRadar.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace AeroRadar.Services;

public class OpenSkyClient : IOpenSkyClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenSkyOptions _opciones;

    public OpenSkyClient(HttpClient httpClient, IOptions<OpenSkyOptions> opciones)
    {
        _httpClient = httpClient;
        _opciones = opciones.Value;
    }

    public async Task<ResultadoConsultaOpenSky> ObtenerAvionesAsync(CancellationToken cancellationToken = default)
    {
        var zona = _opciones.Zona;
        var url = FormattableString.Invariant(
            $"states/all?lamin={zona.LatitudMin}&lomin={zona.LongitudMin}&lamax={zona.LatitudMax}&lomax={zona.LongitudMax}");

        using var respuesta = await _httpClient.GetAsync(url, cancellationToken);

        if (respuesta.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var segundosEspera = LeerCabeceraEntera(respuesta, "X-Rate-Limit-Retry-After-Seconds") ?? 300;
            throw new CuotaAgotadaException(TimeSpan.FromSeconds(segundosEspera));
        }

        respuesta.EnsureSuccessStatusCode();

        var creditosRestantes = LeerCabeceraEntera(respuesta, "X-Rate-Limit-Remaining");
        var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaEstadosOpenSky>(cancellationToken);

        List<Avion> aviones = datos?.Estados is null
            ? []
            : datos.Estados
                .Select(ConvertirAAvion)
                .Where(avion => avion.Latitud is not null && avion.Longitud is not null)
                .ToList();

        return new ResultadoConsultaOpenSky(aviones, creditosRestantes);
    }

    private static Avion ConvertirAAvion(JsonElement[] estado)
    {
        var indicativo = LeerTexto(estado, 1)?.Trim();

        return new Avion
        {
            Icao24 = LeerTexto(estado, 0) ?? string.Empty,
            Indicativo = string.IsNullOrWhiteSpace(indicativo) ? null : indicativo,
            PaisOrigen = LeerTexto(estado, 2) ?? string.Empty,
            UltimaPosicionUnix = LeerEntero(estado, 3),
            Longitud = LeerDecimal(estado, 5),
            Latitud = LeerDecimal(estado, 6),
            AltitudMetros = LeerDecimal(estado, 7),
            EnTierra = LeerBooleano(estado, 8),
            VelocidadMs = LeerDecimal(estado, 9),
            Rumbo = LeerDecimal(estado, 10),
            VelocidadVerticalMs = LeerDecimal(estado, 11)
        };
    }

    private static string? LeerTexto(JsonElement[] estado, int indice) =>
        estado[indice].ValueKind == JsonValueKind.String ? estado[indice].GetString() : null;

    private static double? LeerDecimal(JsonElement[] estado, int indice) =>
        estado[indice].ValueKind == JsonValueKind.Number ? estado[indice].GetDouble() : null;

    private static long? LeerEntero(JsonElement[] estado, int indice) =>
        estado[indice].ValueKind == JsonValueKind.Number ? estado[indice].GetInt64() : null;

    private static bool LeerBooleano(JsonElement[] estado, int indice) =>
        estado[indice].ValueKind == JsonValueKind.True;

    private static int? LeerCabeceraEntera(HttpResponseMessage respuesta, string nombre) =>
    respuesta.Headers.TryGetValues(nombre, out var valores)
    && int.TryParse(valores.FirstOrDefault(), out var numero)
        ? numero
        : null;
}