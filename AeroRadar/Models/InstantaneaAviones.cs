namespace AeroRadar.Models;

public record InstantaneaAviones(IReadOnlyList<Avion> Aviones, DateTimeOffset? ActualizadoUtc);