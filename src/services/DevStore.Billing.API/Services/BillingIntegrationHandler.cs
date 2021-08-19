using DevStore.Billing.API.Models;
using DevStore.Core.DomainObjects;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevStore.Billing.API.Services
{
    public class BillingIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public BillingIntegrationHandler(
                            IServiceProvider serviceProvider,
                            IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        private void SetResponse()
        {
            _bus.RespondAsync<OrderInitiatedIntegrationEvent, ResponseMessage>(async request =>
                await AuthorizeTransaction(request));
        }

        private void SetSubscribers()
        {
            _bus.SubscribeAsync<OrderCanceledIntegrationEvent>("PedidoCancelado", async request =>
            await CancelTransaction(request));

            _bus.SubscribeAsync<OrderLoweredStockIntegrationEvent>("PedidoBaixadoEstoque", async request =>
            await CapturarPagamento(request));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetResponse();
            SetSubscribers();
            return Task.CompletedTask;
        }

        private async Task<ResponseMessage> AuthorizeTransaction(OrderInitiatedIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
            var transaction = new Payment
            {
                OrderId = message.PedidoId,
                PaymentType = (PaymentType)message.TipoPagamento,
                Amount = message.Valor,
                CreditCard = new CreditCard(
                    message.NomeCartao, message.NumeroCartao, message.MesAnoVencimento, message.CVV)
            };

            var response = await billingService.AuthorizeTransaction(transaction);

            return response;
        }

        private async Task CancelTransaction(OrderCanceledIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pagamentoService = scope.ServiceProvider.GetRequiredService<IBillingService>();

                var Response = await pagamentoService.CancelTransaction(message.PedidoId);

                if (!Response.ValidationResult.IsValid)
                    throw new DomainException($"Falha ao cancelar payment do pedido {message.PedidoId}");
            }
        }

        private async Task CapturarPagamento(OrderLoweredStockIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pagamentoService = scope.ServiceProvider.GetRequiredService<IBillingService>();

                var Response = await pagamentoService.GetTransaction(message.PedidoId);

                if (!Response.ValidationResult.IsValid)
                    throw new DomainException($"Falha ao capturar payment do pedido {message.PedidoId}");

                await _bus.PublishAsync(new OrderPaidIntegrationEvent(message.ClienteId, message.PedidoId));
            }
        }
    }
}