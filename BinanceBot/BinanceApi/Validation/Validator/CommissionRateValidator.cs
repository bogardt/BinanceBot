using BinanceBot.BinanceApi.Model;
using FluentValidation;
using System.Globalization;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class CommissionRateValidator : AbstractValidator<CommissionRate>
{
    public CommissionRateValidator()
    {
        When(x => x is not null, () =>
        {
            RuleFor(x => x.Maker)
                .NotNull()
                .NotEmpty()
                .Must(BeAValidDecimal)
                .WithMessage("Should be decimal.");
            RuleFor(x => x.Taker)
                .NotNull()
                .NotEmpty()
                .Must(BeAValidDecimal)
                .WithMessage("Should be decimal.");
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }

    private bool BeAValidDecimal(string value) =>
        decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
}
