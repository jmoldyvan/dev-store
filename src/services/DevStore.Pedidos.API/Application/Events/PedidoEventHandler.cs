using System.Threading;
using System.Threading.Tasks;
using DevStore.Core.Messages.Integration;
using DevStore.MessageBus;
using MediatR;

namespace DevStore.Orders.API.Application.Events
{
    public class PedidoEventHandler : INotificationHandler<PedidoRealizadoEvent>
    {
        private readonly IMessageBus _bus;

        public PedidoEventHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(PedidoRealizadoEvent message, CancellationToken cancellationToken)
        {
            await _bus.PublishAsync(new OrderDoneIntegrationEvent(message.ClienteId));
        }
    }
}