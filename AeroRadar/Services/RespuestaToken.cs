using System.Text.Json.Serialization;

namespace AeroRadar.Services;

public class RespuestaToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiraEnSegundos { get; set; }
}