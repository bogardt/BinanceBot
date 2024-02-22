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
            RuleFor(x => x.OrderId).Null();
            RuleFor(x => x.Symbol).Null();
            RuleFor(x => x.Side).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
