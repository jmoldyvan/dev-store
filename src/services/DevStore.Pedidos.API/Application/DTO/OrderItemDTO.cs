using System;
using DevStore.Orders.Domain.Pedidos;

namespace DevStore.Orders.API.Application.DTO
{
    public class OrderItemDTO
    {
        public Guid PedidoId { get; set; }
        public Guid ProdutoId { get; set; }
        public string Nome { get; set; }
        public decimal Valor { get; set; }
        public string Imagem { get; set; }
        public int Quantidade { get; set; }

        public static OrderItem ParaPedidoItem(OrderItemDTO orderItemDto)
        {
            return new OrderItem(orderItemDto.ProdutoId, orderItemDto.Nome, orderItemDto.Quantidade,
                orderItemDto.Valor, orderItemDto.Imagem);
        }
    }
}