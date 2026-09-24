using AeroRadar.Configuration;
using Microsoft.Extensions.Options;
using AeroRadar.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AeroRadar.Services;

public class ConsultaAvionesService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlmacenAviones _almacen;
    private readonly OpenSkyOptions _opciones;
    private readonly ILogger<ConsultaAvionesService> _logger;
    private readonly IHubContext<AvionesHub, IAvionesCliente> _hubContext;
    private readonly ContadorConexiones _contador;

    public ConsultaAvionesService(
    IServiceScopeFactory scopeFactory,
    AlmacenAviones almacen,
    IHubContext<AvionesHub, IAvionesCliente> hubContext,
    IOptions<OpenSkyOptions> opciones,
    ILogger<ConsultaAvionesService> logger,
    ContadorConexiones contador)
    {
        _scopeFactory = scopeFactory;
        _almacen = almacen;
        _hubContext = hubContext;
        _opciones = opciones.Value;
        _logger = logger;
        _contador = contador;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromSeconds(_opciones.IntervaloConsultaSegundos);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_contador.Conexiones == 0)
            {
                _logger.LogInformation("Sin clientes conectados. Consultas en pausa");
                await _contador.EsperarConexionAsync(stoppingToken);
                _logger.LogInformation("Cliente conectado. Reanudando consultas");
            }

            await ConsultarAsync(stoppingToken);
            await Task.Delay(intervalo, stoppingToken);
        }
    }

    private async Task ConsultarAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cliente = scope.ServiceProvider.GetRequiredService<IOpenSkyClient>();

            var aviones = await cliente.ObtenerAvionesAsync(stoppingToken);
            _almacen.Actualizar(aviones);

            await _hubContext.Clients.All.RecibirAviones(_almacen.Actual);

            _logger.LogInformation("Consulta completada: {Total} aviones", aviones.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // La aplicación se está cerrando: no es un error.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar OpenSky");
        }
    }
}