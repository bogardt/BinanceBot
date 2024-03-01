using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class DiscountValidator : AbstractValidator<Discount>
{
    public DiscountValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.DiscountAsset).NotNull().NotEmpty().Equal("BNB");
            RuleFor(x => x.DiscountValue).NotNull().NotEmpty().Equal("0.75000000");
            RuleFor(x => x.EnabledForAccount).Equal(true);
            RuleFor(x => x.EnabledForSymbol).Equal(true);
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
