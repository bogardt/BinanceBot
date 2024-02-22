using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class CurrencyValidator : AbstractValidator<Currency>
{
    public CurrencyValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
            RuleFor(x => x.Price).NotNull().NotEmpty();
            RuleFor(x => x.Symbol).NotNull().NotEmpty();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
