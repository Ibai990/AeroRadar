using System.Text.Json;
using System.Text.Json.Serialization;

namespace AeroRadar.Services;

public class RespuestaEstadosOpenSky
{
    [JsonPropertyName("time")]
    public long Tiempo { get; set; }

    [JsonPropertyName("states")]
    public List<JsonElement[]>? Estados { get; set; }

}
