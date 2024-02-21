using BinanceBot.BinanceApi.Validation;
using System.Runtime.Serialization;

namespace BinanceBot.Exceptions;

[Serializable]
internal class ValidationException : Exception
{
    private List<ValidationError> _validationErrors;

    public ValidationException()
    {
    }

    public ValidationException(List<ValidationError> validationErrors)
    {
        _validationErrors = validationErrors;
    }

    public ValidationException(string? message) : base(message)
    {
    }

    public ValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}