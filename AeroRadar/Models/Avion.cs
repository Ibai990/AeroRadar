namespace AeroRadar.Models;

public class Avion
{
    public string Icao24 { get; set; } = string.Empty;
    public string? Indicativo { get; set; }
    public string PaisOrigen { get; set; } = string.Empty;
    public long? UltimaPosicionUnix { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public double? AltitudMetros { get; set; }
    public bool EnTierra { get; set; }
    public double? VelocidadMs { get; set; }
    public double? Rumbo { get; set; }
    public double? VelocidadVerticalMs { get; set; }
}