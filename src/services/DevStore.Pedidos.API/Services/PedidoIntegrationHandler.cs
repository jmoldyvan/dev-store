using System;
using System.Threading;
using System.Threading.Tasks;
using DevStore.Core.DomainObjects;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using DevStore.Orders.Domain.Pedidos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevStore.Orders.API.Services
{
    public class PedidoIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public PedidoIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetSubscribers();
            return Task.CompletedTask;
        }

        private void SetSubscribers()
        {
            _bus.SubscribeAsync<OrderCanceledIntegrationEvent>("PedidoCancelado",
                async request => await CancelarPedido(request));

            _bus.SubscribeAsync<OrderPaidIntegrationEvent>("PedidoPago",
               async request => await FinalizarPedido(request));
        }

        private async Task CancelarPedido(OrderCanceledIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pedidoRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var pedido = await pedidoRepository.GetById(message.PedidoId);
                pedido.CancelarPedido();

                pedidoRepository.Update(pedido);

                if (!await pedidoRepository.UnitOfWork.Commit())
                {
                    throw new DomainException($"Problemas ao cancelar o order {message.PedidoId}");
                }
            }
        }

        private async Task FinalizarPedido(OrderPaidIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pedidoRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var pedido = await pedidoRepository.GetById(message.PedidoId);
                pedido.FinalizarPedido();

                pedidoRepository.Update(pedido);

                if (!await pedidoRepository.UnitOfWork.Commit())
                {
                    throw new DomainException($"Problemas ao finalizar o order {message.PedidoId}");
                }
            }
        }
    }
}