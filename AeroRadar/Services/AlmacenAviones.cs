using AeroRadar.Models;

namespace AeroRadar.Services;

public class AlmacenAviones
{
    private InstantaneaAviones _actual = new([], null);

    public InstantaneaAviones Actual => Volatile.Read(ref _actual);

    public void Actualizar(IReadOnlyList<Avion> aviones)
    {
        Volatile.Write(ref _actual, new InstantaneaAviones(aviones, DateTimeOffset.UtcNow));
    }
}