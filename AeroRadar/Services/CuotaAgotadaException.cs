namespace AeroRadar.Services;

public class CuotaAgotadaException : Exception
{
    public TimeSpan EsperaRecomendada { get; }

    public CuotaAgotadaException(TimeSpan esperaRecomendada)
        : base($"Cuota de OpenSky agotada. Reintentar en {esperaRecomendada}")
    {
        EsperaRecomendada = esperaRecomendada;
    }
}