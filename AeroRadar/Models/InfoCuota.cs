namespace AeroRadar.Models;

public record InfoCuota(int? CreditosRestantes, TimeSpan IntervaloActual, DateTimeOffset? PausadoHastaUtc);