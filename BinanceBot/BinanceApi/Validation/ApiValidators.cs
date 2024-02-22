using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Validation;

public class ApiValidators<MessageType> : IApiValidator<MessageType> where MessageType : BaseMessage
{
    public ApiValidators(IValidator<MessageType> validator)
    {
        _validator = validator;
    }
    private readonly IValidator<MessageType> _validator;

    public async Task<MessageType> ValidateResponse(HttpResponseMessage? response)
    {
        var res = await response!.Content.ReadAsStringAsync();
        var message = JsonConvert.DeserializeObject<MessageType>(res) ?? default;
        _validator.Validate<MessageType>(message, (option) => { });
        return message!;
    }

    public async Task<IEnumerable<MessageType>> Validate1DResponse(HttpResponseMessage? response)
    {
        var res = await response!.Content.ReadAsStringAsync();
        var messages = (JsonConvert.DeserializeObject<List<MessageType>>(res) ?? null)
            ?? throw new Exceptions.ValidationException("messages is null");

        foreach (var message in messages)
            _validator.Validate((IValidationContext)message);

        return messages!;
    }

    public async Task<IEnumerable<IEnumerable<object>>> Validate2DMatriceResponse(HttpResponseMessage? response)
    {
        var res = await response!.Content.ReadAsStringAsync();
        var firsts = JsonConvert.DeserializeObject<List<List<object>>>(res) ?? null;

        //if (firsts is not null)
        //    foreach (var first in firsts)
        //        foreach (var second in first) ;
        ////_abstractValidatorMessageService.ValidateBase(second);
        //else
        //    throw new Exceptions.ValidationException("messages is null");

        return firsts!;
    }

}
