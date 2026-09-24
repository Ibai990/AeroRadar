namespace AeroRadar.Configuration
{
    public class OpenSkyOptions
    {
        public const string SectionName = "OpenSky";

        public string UrlBase { get; set; } = string.Empty;
        public string UrlToken { get; set; } = string.Empty;
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        public bool TieneCredenciales =>
            !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);

        public int CreditosDiarios => TieneCredenciales ? 4000 : 400;

        public int IntervaloConsultaSegundos { get; set; } = 30;
        public ZonaConsulta Zona { get; set; } = new();
    }
}
