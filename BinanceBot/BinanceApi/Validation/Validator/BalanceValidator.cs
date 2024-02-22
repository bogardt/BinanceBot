using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class BalanceValidator : AbstractValidator<Balance>
{
    public BalanceValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Asset).NotNull().NotEmpty();
            RuleFor(x => x.Free).NotNull().NotEmpty();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
