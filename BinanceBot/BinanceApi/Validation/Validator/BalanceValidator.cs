using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class BalanceValidator : AbstractValidator<Balance>
{
    public BalanceValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Asset).Null();
            RuleFor(x => x.Free).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
