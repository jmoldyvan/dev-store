using System.Threading.Tasks;
using DevStore.Orders.API.Application.DTO;
using DevStore.Orders.Domain.Vouchers;

namespace DevStore.Orders.API.Application.Queries
{
    public interface IVoucherQueries
    {
        Task<VoucherDTO> ObterVoucherPorCodigo(string codigo);
    }

    public class VoucherQueries : IVoucherQueries
    {
        private readonly IVoucherRepository _voucherRepository;

        public VoucherQueries(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<VoucherDTO> ObterVoucherPorCodigo(string codigo)
        {
            var voucher = await _voucherRepository.ObterVoucherPorCodigo(codigo);

            if (voucher == null) return null;

            if (!voucher.CanUse()) return null;

            return new VoucherDTO
            {
                Codigo = voucher.Code,
                TipoDesconto = (int)voucher.DiscountType,
                Percentual = voucher.Percentage,
                ValorDesconto = voucher.Discount
            };
        }
    }
}