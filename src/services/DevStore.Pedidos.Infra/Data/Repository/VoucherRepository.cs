using System.Threading.Tasks;
using DevStore.Core.Data;
using DevStore.Orders.Domain.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace DevStore.Orders.Infra.Data.Repository
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly OrdersContext _context;

        public VoucherRepository(OrdersContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<Voucher> ObterVoucherPorCodigo(string codigo)
        {
            return await _context.Vouchers.FirstOrDefaultAsync(p => p.Code == codigo);
        }

        public void Atualizar(Voucher voucher)
        {
            _context.Vouchers.Update(voucher);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}