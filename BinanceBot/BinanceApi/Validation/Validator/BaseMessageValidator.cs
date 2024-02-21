using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation.Validator;

public class BaseMessageValidator : AbstractValidator<BaseMessage>
{
    public BaseMessageValidator()
    {
        When(x => x is not null, () =>
        {
            //RuleFor(x => x.Asset).Null();
            //RuleFor(x => x.Free).Null();
        }).Otherwise(() =>
        {
            RuleFor(x => x).NotNull();
        });
    }
}
