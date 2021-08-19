using Dapper;
using DevStore.Orders.API.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevStore.Orders.Domain.Pedidos;

namespace DevStore.Orders.API.Application.Queries
{
    public interface IPedidoQueries
    {
        Task<OrderDTO> ObterUltimoPedido(Guid clienteId);
        Task<IEnumerable<OrderDTO>> ObterListaPorClienteId(Guid clienteId);
        Task<OrderDTO> ObterPedidosAutorizados();
    }

    public class PedidoQueries : IPedidoQueries
    {
        private readonly IOrderRepository _orderRepository;

        public PedidoQueries(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDTO> ObterUltimoPedido(Guid clienteId)
        {
            const string sql = @"SELECT
                                P.ID AS 'PRODUCTID', P.CODE, P.HASVOUCHER, P.DISCOUNT, P.AMOUNT,P.ORDERSTATUS,
                                P.STREETADDRESS,P.BUILDINGNUMBER, P.NEIGHBORHOOD, P.ZIPCODE, P.SECONDARYADDRESS, P.CITY, P.STATE,
                                PIT.ID AS 'PRODUCTITEMID',PIT.PRODUCTNAME, PIT.QUANTITY, PIT.PRODUCTIMAGE, PIT.PRICE 
                                FROM Orders P 
                                INNER JOIN OrderITEMS PIT ON P.ID = PIT.OrderID 
                                WHERE P.CLIENTID = @clienteId 
                                AND P.DateAdded between DATEADD(minute, -5,  @Now) and @Now
                                ORDER BY P.DateAdded DESC";

            var pedido = await _orderRepository.GetConnection()
                .QueryAsync<dynamic>(sql, new { clienteId, Now = DateTime.Now });

            if (!pedido.Any())
                return null;

            return MapOrder(pedido);
        }

        public async Task<IEnumerable<OrderDTO>> ObterListaPorClienteId(Guid clienteId)
        {
            var pedidos = await _orderRepository.GetClientsById(clienteId);

            return pedidos.Select(OrderDTO.ToOrderDTO);
        }

        public async Task<OrderDTO> ObterPedidosAutorizados()
        {
            // Correção para pegar todos os itens do order e ordernar pelo order mais antigo
            const string sql = @"SELECT 
                                P.ID as 'OrderId', P.ID, P.CLIENTEID, 
                                PI.ID as 'PedidoItemId', PI.ID, PI.PRODUTOID, PI.QUANTIDADE 
                                FROM PEDIDOS P 
                                INNER JOIN PEDIDOITEMS PI ON P.ID = PI.PEDIDOID 
                                WHERE P.PEDIDOSTATUS = 1                                
                                ORDER BY P.DATACADASTRO";

            // Utilizacao do lookup para manter o estado a cada ciclo de registro retornado
            var lookup = new Dictionary<Guid, OrderDTO>();

            await _orderRepository.GetConnection().QueryAsync<OrderDTO, OrderItemDTO, OrderDTO>(sql,
                (p, pi) =>
                {
                    if (!lookup.TryGetValue(p.Id, out var pedidoDTO))
                        lookup.Add(p.Id, pedidoDTO = p);

                    pedidoDTO.OrderItems ??= new List<OrderItemDTO>();
                    pedidoDTO.OrderItems.Add(pi);

                    return pedidoDTO;

                }, splitOn: "OrderId,PedidoItemId");

            // Obtendo dados o lookup
            return lookup.Values.OrderBy(p => p.Data).FirstOrDefault();
        }

        private OrderDTO MapOrder(dynamic result)
        {
            var pedido = new OrderDTO
            {
                Code = result[0].CODE,
                Status = result[0].ORDERSTATUS,
                Amount = result[0].AMOUNT,
                Discount = result[0].DISCOUNT,
                HasVoucher = result[0].HASVOUCHER,

                OrderItems = new List<OrderItemDTO>(),
                Address = new AddressDto
                {
                    StreetAddress = result[0].STREETADDRESS,
                    Neighborhood = result[0].NEIGHBORHOOD,
                    ZipCode = result[0].ZIPCODE,
                    City = result[0].CITY,
                    SecondaryAddress = result[0].SECONDARYADDRESS,
                    State = result[0].STATE,
                    BuildingNumber = result[0].BUILDINGNUMBER
                }
            };

            foreach (var item in result)
            {
                var pedidoItem = new OrderItemDTO
                {
                    Nome = item.PRODUCTNAME,
                    Valor = item.PRICE,
                    Quantidade = item.QUANTITY,
                    Imagem = item.PRODUCTIMAGE
                };

                pedido.OrderItems.Add(pedidoItem);
            }

            return pedido;
        }
    }

}