using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class TestOrderValidator : AbstractValidator<TestOrder>
{
    public TestOrderValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Code).Null();
            RuleFor(x => x.Message).Null();
            RuleFor(x => x.StandardCommissionForOrder).SetValidator(new CommissionRateValidator());
            RuleFor(x => x.TaxCommissionForOrder).SetValidator(new CommissionRateValidator());
            RuleFor(x => x.Discount).SetValidator(new DiscountValidator());
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
