using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class AcountValidator : AbstractValidator<Account>
{
    public AcountValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Balances).ForEach(x => x.SetValidator(new BalanceValidator()));
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
