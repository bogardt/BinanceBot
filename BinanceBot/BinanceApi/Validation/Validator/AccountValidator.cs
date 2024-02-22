using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class AccountValidator : AbstractValidator<Account>
{
    public AccountValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
            RuleFor(x => x.Balances).ForEach(x => x.SetValidator(new BalanceValidator()));
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
