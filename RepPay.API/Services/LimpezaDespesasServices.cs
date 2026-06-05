using Microsoft.EntityFrameworkCore;
using RepPay.API.Models;

namespace RepPay.API.Services
{
    public class LimpezaDespesasService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LimpezaDespesasService> _logger;

        public LimpezaDespesasService(IServiceProvider serviceProvider, ILogger<LimpezaDespesasService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de limpeza de despesas antigas iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        var dataCorte = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));


                        int despesasDeletadas = await context.Despesas
                            .Where(d => d.DataCadastro <= dataCorte)
                            .ExecuteDeleteAsync(stoppingToken);

                        if (despesasDeletadas > 0)
                        {
                            _logger.LogInformation($"RF12 Executado: {despesasDeletadas} despesas com mais de 1 ano foram excluídas.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar limpar despesas antigas.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}