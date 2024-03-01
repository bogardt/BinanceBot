using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using FluentValidation;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Validation;

public class ApiValidatorService : IApiValidatorService
{
    private readonly IServiceProvider _serviceProvider;

    public ApiValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<T> ValidateAsync<T>(string json) where T : IMessage
    {
        var message = JsonConvert.DeserializeObject<T>(json) ?? default;

        return await Validate(message);
    }

    private IValidator GetValidatorByType<T>(T message) where T : IMessage
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(message!.GetType());
        var validator = (IValidator)_serviceProvider.GetService(validatorType)!;
        if (validator == null)
            throw new InvalidOperationException($"No validator found for type {message.GetType().Name}.");
        return validator;
    }

    private async Task<T> Validate<T>(T? message) where T : IMessage
    {
        var validator = GetValidatorByType(message!);
        var context = new ValidationContext<T>(message);
        var validationResult = await validator.ValidateAsync(context);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        return message;
    }
}
