using AeroRadar.Configuration;
using AeroRadar.Hubs;
using AeroRadar.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AeroRadar.Services;

public class ConsultaAvionesService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlmacenAviones _almacen;
    private readonly OpenSkyOptions _opciones;
    private readonly ILogger<ConsultaAvionesService> _logger;
    private readonly IHubContext<AvionesHub, IAvionesCliente> _hubContext;
    private readonly ContadorConexiones _contador;
    private readonly EstadoCuota _estadoCuota;

    public ConsultaAvionesService(
    IServiceScopeFactory scopeFactory,
    AlmacenAviones almacen,
    IHubContext<AvionesHub, IAvionesCliente> hubContext,
    IOptions<OpenSkyOptions> opciones,
    ILogger<ConsultaAvionesService> logger,
    ContadorConexiones contador, EstadoCuota estadoCuota)
    {
        _scopeFactory = scopeFactory;
        _almacen = almacen;
        _hubContext = hubContext;
        _opciones = opciones.Value;
        _logger = logger;
        _contador = contador;
        _estadoCuota = estadoCuota;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_contador.Conexiones == 0)
            {
                _logger.LogInformation("Sin clientes conectados. Consultas en pausa");
                await _contador.EsperarConexionAsync(stoppingToken);
                _logger.LogInformation("Cliente conectado. Reanudando consultas");
            }

            var espera = await ConsultarAsync(stoppingToken);
            await Task.Delay(espera, stoppingToken);
        }
    }

    private async Task<TimeSpan> ConsultarAsync(CancellationToken stoppingToken)
    {
        var intervaloBase = TimeSpan.FromSeconds(_opciones.IntervaloConsultaSegundos);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cliente = scope.ServiceProvider.GetRequiredService<IOpenSkyClient>();

            var resultado = await cliente.ObtenerAvionesAsync(stoppingToken);
            _almacen.Actualizar(resultado.Aviones);
            await _hubContext.Clients.All.RecibirAviones(_almacen.Actual);

            var intervalo = CalcularIntervalo(resultado.CreditosRestantes, intervaloBase);
            _estadoCuota.Actualizar(new InfoCuota(resultado.CreditosRestantes, intervalo, null));

            _logger.LogInformation(
                "Consulta completada: {Total} aviones. Créditos restantes: {Creditos}. Próxima consulta en {Segundos} s",
                resultado.Aviones.Count, resultado.CreditosRestantes, intervalo.TotalSeconds);

            return intervalo;
        }
        catch (CuotaAgotadaException ex)
        {
            var pausadoHasta = DateTimeOffset.UtcNow + ex.EsperaRecomendada;
            _estadoCuota.Actualizar(new InfoCuota(0, ex.EsperaRecomendada, pausadoHasta));

            _logger.LogWarning("Cuota de OpenSky agotada. Consultas en pausa hasta {Hora}", pausadoHasta);
            return ex.EsperaRecomendada;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return TimeSpan.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar OpenSky");
            return intervaloBase;
        }
    }

    private TimeSpan CalcularIntervalo(int? creditosRestantes, TimeSpan intervaloBase)
    {
        if (creditosRestantes is null)
        {
            return intervaloBase;
        }

        var porcentaje = (double)creditosRestantes.Value / _opciones.CreditosDiarios;

        var multiplicador = porcentaje switch
        {
            >= 0.25 => 1,
            >= 0.10 => 2,
            >= 0.03 => 4,
            _ => 8
        };

        return intervaloBase * multiplicador;
    }
}