using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
            RuleFor(x => x.OrderId).NotNull().NotEmpty();
            RuleFor(x => x.Symbol).NotNull().NotEmpty();
            RuleFor(x => x.Side).NotNull().NotEmpty();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
