using AeroRadar.Models;

namespace AeroRadar.Services;

public record ResultadoConsultaOpenSky(List<Avion> Aviones, int? CreditosRestantes);