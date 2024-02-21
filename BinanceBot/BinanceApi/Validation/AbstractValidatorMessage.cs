using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;

namespace BinanceBot.BinanceApi.Validation;

public abstract class AbstractValidatorMessage<T> : AbstractValidator<T> where T : BaseMessage?
{
    public AbstractValidatorMessage()
    {
    }

    // changer la fonction par l'overide de Validate de AbstractValidator
    public void ValidateBase(BaseMessage? baseMessage)
    {
        if (baseMessage is null)
            throw new Exceptions.ValidationException("message is null");

        var validate = Validate((T)baseMessage);
        if (baseMessage.Code is not null && baseMessage.Message is not null)
            throw new Exceptions.ValidationException($"Unable to get infos code [{baseMessage.Code}] : {baseMessage.Message}");

        if (!validate.IsValid)
        {
            var validationErrors = validate.Errors
                .Select(error => new ValidationError(error.PropertyName, error.ErrorMessage))
                .ToList();

            throw new Exceptions.ValidationException(validationErrors);
        }
    }
}
