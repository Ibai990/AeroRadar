namespace AeroRadar.Configuration
{
    public class ZonaConsulta
    {
        public double LatitudMin { get; set; }
        public double LatitudMax { get; set; }
        public double LongitudMin { get; set; }
        public double LongitudMax { get; set; }

        public double AreaGradosCuadrados => (LatitudMax - LatitudMin) * (LongitudMax - LongitudMin);
    }
}
