using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using DevStore.Orders.API.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DevStore.Orders.API.Services
{
    public class PedidoOrquestradorIntegrationHandler : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PedidoOrquestradorIntegrationHandler> _logger;
        private Timer _timer;

        public PedidoOrquestradorIntegrationHandler(ILogger<PedidoOrquestradorIntegrationHandler> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de pedidos iniciado.");

            _timer = new Timer(ProcessarPedidos, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(15));

            return Task.CompletedTask;
        }

        private async void ProcessarPedidos(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pedidoQueries = scope.ServiceProvider.GetRequiredService<IPedidoQueries>();
                var pedido = await pedidoQueries.ObterPedidosAutorizados();

                if (pedido == null) return;

                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var authorizedOrder = new OrderAuthorizedIntegrationEvent(pedido.ClientId, pedido.Id,
                    pedido.OrderItems.ToDictionary(p => p.ProdutoId, p => p.Quantidade));

                await bus.PublishAsync(authorizedOrder);

                _logger.LogInformation($"Order ID: {pedido.Id} foi encaminhado para baixa no estoque.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Serviço de pedidos finalizado.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}