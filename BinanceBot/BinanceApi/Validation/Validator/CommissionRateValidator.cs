using BinanceBot.BinanceApi.Model;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class CommissionRateValidator : AbstractValidator<CommissionRate>
{
    public CommissionRateValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Maker).NotNull().NotEmpty();
            RuleFor(x => x.Taker).NotNull().NotEmpty();
            //RuleFor(x => x.Buyer).NotNull().NotEmpty();
            //RuleFor(x => x.Seller).NotNull().NotEmpty();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
