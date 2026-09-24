using System.Text.Json;
using AeroRadar.Configuration;
using AeroRadar.Models;
using Microsoft.Extensions.Options;

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

    public async Task<List<Avion>> ObtenerAvionesAsync(CancellationToken cancellationToken = default)
    {
        var zona = _opciones.Zona;
        var url = FormattableString.Invariant(
            $"states/all?lamin={zona.LatitudMin}&lomin={zona.LongitudMin}&lamax={zona.LatitudMax}&lomax={zona.LongitudMax}");

        var respuesta = await _httpClient.GetFromJsonAsync<RespuestaEstadosOpenSky>(url, cancellationToken);

        if (respuesta?.Estados is null)
        {
            return [];
        }

        return respuesta.Estados
            .Select(ConvertirAAvion)
            .Where(avion => avion.Latitud is not null && avion.Longitud is not null)
            .ToList();
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
}