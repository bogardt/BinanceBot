using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class CommissionValidator : AbstractValidator<Commission>
{
    public CommissionValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
            RuleFor(x => x.Discount).SetValidator(new DiscountValidator());
            RuleFor(x => x.StandardCommission).SetValidator(new CommissionRateValidator());
            RuleFor(x => x.TaxCommission).SetValidator(new CommissionRateValidator());
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
