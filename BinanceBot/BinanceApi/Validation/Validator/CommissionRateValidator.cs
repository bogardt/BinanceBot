using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class CommissionRateValidator : AbstractValidator<CommissionRate>
{
    public CommissionRateValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Maker).Null();
            RuleFor(x => x.Taker).Null();
            RuleFor(x => x.Buyer).Null();
            RuleFor(x => x.Seller).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
