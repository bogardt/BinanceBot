
using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using FluentValidation;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Validation;

public class ApiValidatorService : IApiValidatorService
{
    private readonly Dictionary<Type, IValidator> _validators;

    private IValidator _validator;
    public  async Task<IMessage> ValidationType(IMessage message, string json) // will do it later
    {
        message = message switch
        {
            Account account => await ValidateAsync<Account>(json),
            Balance balance => await ValidateAsync<Balance>(json),
            Commission commission => await ValidateAsync<Commission>(json),
            CommissionRate commissionRate => await ValidateAsync<CommissionRate>(json),
            Discount discount => await ValidateAsync<Discount>(json),
            Order accountValidator => await ValidateAsync<Order>(json),
            TestOrder accountValidator => await ValidateAsync<TestOrder>(json),
            _ => message
        };

        return message;
    }

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

    public async Task<T> ValidateAsync<T>(string json) where T : IMessage
    {
        var message = JsonConvert.DeserializeObject<T>(json) ?? default;

        return await Validate(message);
    }

    private async Task<T> Validate<T>(T? message)
    {
        if (_validators.TryGetValue(typeof(T), out var validator))
        {
            var validationResult = await ((IValidator<T>)validator).ValidateAsync(message);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            return message!;
        }
        else
            throw new InvalidOperationException($"Missing indicator for type {typeof(T).Name}.");
    }
}
