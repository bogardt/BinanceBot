using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class DiscountValidator : AbstractValidator<Discount>
{
    public DiscountValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.DiscountValue).Null();
            RuleFor(x => x.DiscountAsset).Null();
            RuleFor(x => x.EnabledForAccount).Null();
            RuleFor(x => x.EnabledForSymbol).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
