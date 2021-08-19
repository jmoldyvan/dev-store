using System;
using System.Collections.Generic;
using DevStore.Core.Messages;
using DevStore.Orders.API.Application.DTO;
using FluentValidation;

namespace DevStore.Orders.API.Application.Commands
{
    public class AddOrderCommand : Command
    {
        // Order
        public Guid ClientId { get; set; }
        public decimal Amount { get; set; }
        public List<OrderItemDTO> PedidoItems { get; set; }

        // Voucher
        public string Voucher { get; set; }
        public bool HasVoucher { get; set; }
        public decimal Discount { get; set; }

        // Address
        public AddressDto Address { get; set; }

        // Cartao
        public string CardNumber { get; set; }
        public string Holder { get; set; }
        public string ExpirationMonth { get; set; }
        public string SecurityCode { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new AddOrderValidation().Validate(this);
            return ValidationResult.IsValid;
        }

        public class AddOrderValidation : AbstractValidator<AddOrderCommand>
        {
            public AddOrderValidation()
            {
                RuleFor(c => c.ClientId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("Invalid client id");

                RuleFor(c => c.PedidoItems.Count)
                    .GreaterThan(0)
                    .WithMessage("The order needs to have at least 1 item");

                RuleFor(c => c.Amount)
                    .GreaterThan(0)
                    .WithMessage("Invalid order amount");

                RuleFor(c => c.CardNumber)
                    .CreditCard()
                    .WithMessage("Invalid credit card");

                RuleFor(c => c.Holder)
                    .NotNull()
                    .WithMessage("Holder name is required.");

                RuleFor(c => c.SecurityCode.Length)
                    .GreaterThan(2)
                    .LessThan(5)
                    .WithMessage("The security code must have at least 3 or 4 numbers.");

                RuleFor(c => c.ExpirationMonth)
                    .NotNull()
                    .WithMessage("Expiration date is required.");
            }
        }
    }
}