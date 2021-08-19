using System;
using DevStore.Core.DomainObjects;
using DevStore.Orders.Domain.Vouchers.Specs;

namespace DevStore.Orders.Domain.Vouchers
{
    public class Voucher : Entity, IAggregateRoot
    {
        public string Code { get; private set; }
        public decimal? Percentage { get; private set; }
        public decimal? Discount { get; private set; }
        public int Quantity { get; private set; }
        public VoucherDiscountType DiscountType { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public bool Active { get; private set; }
        public bool Used { get; private set; }

        public bool CanUse()
        {
            return new VoucherActiveSpecification()
                .And(new VoucherDateSpecification())
                .And(new VoucherQuantitySpecification())
                .IsSatisfiedBy(this);
        }

        public void SetAsUsed()
        {
            Active = false;
            Used = true;
            Quantity = 0;
            UsedAt = DateTime.Now;
        }

        public void GetOne()
        {
            Quantity -= 1;
            if (Quantity >= 1) return;

            SetAsUsed();
        }
    }
}