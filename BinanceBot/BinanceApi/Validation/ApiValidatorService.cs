
using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using FluentValidation;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Validation;

public class ApiValidatorService : IApiValidatorService
{
    private readonly Dictionary<Type, IValidator> _validators;

    public ApiValidatorService(IServiceProvider serviceProvider)
    {
        _validators = new Dictionary<Type, IValidator>
        {
            { typeof(Account), (IValidator)serviceProvider.GetService(typeof(AccountValidator)) },
            { typeof(Balance), (IValidator)serviceProvider.GetService(typeof(BalanceValidator)) },
            { typeof(Commission), (IValidator)serviceProvider.GetService(typeof(CommissionValidator)) },
            { typeof(CommissionRate), (IValidator)serviceProvider.GetService(typeof(CommissionRateValidator)) },
            { typeof(Currency), (IValidator)serviceProvider.GetService(typeof(CurrencyValidator)) },
            { typeof(Discount), (IValidator)serviceProvider.GetService(typeof(DiscountValidator)) },
            { typeof(Order), (IValidator)serviceProvider.GetService(typeof(OrderValidator)) },
            { typeof(TestOrder), (IValidator)serviceProvider.GetService(typeof(TestOrderValidator)) },
            { typeof(object), (IValidator)serviceProvider.GetService(typeof(ObjectValidator)) }
        };
    }

    public async Task<T> ValidateAsync<T>(HttpResponseMessage? response) where T : BaseMessage
    {
        var res = await response!.Content.ReadAsStringAsync();
        var message = JsonConvert.DeserializeObject<T>(res) ?? default;

        return await Validate(message);
    }

    public async Task<IEnumerable<T>> Validate1DResponse<T>(HttpResponseMessage? response) where T : BaseMessage
    {
        var res = await response!.Content.ReadAsStringAsync();
        var messages = (JsonConvert.DeserializeObject<List<T>>(res) ?? null)
            ?? throw new ValidationException("messages is null");

        foreach (var message in messages)
            await Validate(message);

        return messages!;
    }

    public async Task<IEnumerable<IEnumerable<object>>> Validate2DMatriceResponse(HttpResponseMessage? response)
    {
        var res = await response!.Content.ReadAsStringAsync();
        var firsts = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<object>>>(res) ?? null;

        if (firsts is not null)
            foreach (var first in firsts)
                foreach (var second in first)
                    await Validate(second);
        else
            throw new ValidationException("messages is null");

        return firsts!;
    }

    private async Task<T> Validate<T>(T? message)
    {
        if (_validators.TryGetValue(typeof(T), out var validator))
        {
            var validationResult = await ((IValidator<T>)validator).ValidateAsync(message);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
            return message!;
        }
        else
            throw new InvalidOperationException($"Missing indicator for type {typeof(T).Name}.");
    }

}
