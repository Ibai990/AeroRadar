namespace AeroRadar.Configuration
{
    public class OpenSkyOptions
    {
        public const string SectionName = "OpenSky";

        public string UrlBase { get; set; } = string.Empty;
        public int IntervaloConsultaSegundos { get; set; } = 30;
        public ZonaConsulta Zona { get; set; } = new();
    }
}
