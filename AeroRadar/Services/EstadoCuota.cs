using AeroRadar.Models;

namespace AeroRadar.Services;

public class EstadoCuota
{
    private InfoCuota _actual = new(null, TimeSpan.Zero, null);

    public InfoCuota Actual => Volatile.Read(ref _actual);

    public void Actualizar(InfoCuota info) => Volatile.Write(ref _actual, info);
}