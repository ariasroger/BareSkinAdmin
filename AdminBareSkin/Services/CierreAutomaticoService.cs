
using AdminBareSkin.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Services
{
    public class CierreAutomaticoService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CierreAutomaticoService> _logger;

        public CierreAutomaticoService(
            IServiceScopeFactory scopeFactory,
            ILogger<CierreAutomaticoService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarMesPendienteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error al procesar los cierres mensuales automáticos.");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromHours(24),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcesarMesPendienteAsync()
        {
            var ahora = DateTime.UtcNow;

            // Solo se cierra un mes cuando ya terminó.
            var mesAnterior = new DateTime(
                ahora.Year,
                ahora.Month,
                1,
                0, 0, 0,
                DateTimeKind.Utc).AddMonths(-1);

            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var servicio = scope.ServiceProvider
                .GetRequiredService<CierreMensualService>();

            var existeCierre = await context.CierresMensuales
                .Where(c => c.Anio == mesAnterior.Year &&
                            c.Mes == mesAnterior.Month)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            if (existeCierre?.Estado == "Cerrado")
                return;

            await servicio.CerrarMesAsync(
                mesAnterior.Year,
                mesAnterior.Month,
                "Sistema automático",
                true);
        }
    }
}
