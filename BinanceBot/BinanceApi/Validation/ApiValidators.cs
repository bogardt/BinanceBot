using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model.Message;
using FluentValidation;
using Newtonsoft.Json;

namespace BinanceBot.BinanceApi.Validation;

public class ApiValidators : IApiValidator
{
    public ApiValidators(IValidator<BaseMessage> validator)
    {
        _validator = validator;
    }
    private readonly IValidator<BaseMessage> _validator;

    public async Task<MessageType> ValidateResponse<MessageType>(HttpResponseMessage? response) where MessageType : BaseMessage
    {
        var res = await response!.Content.ReadAsStringAsync();
        var message = JsonConvert.DeserializeObject<MessageType>(res) ?? default;
        _validator.Validate(message);
        return message!;
    }

    public async Task<IEnumerable<MessageType>> Validate1DResponse<MessageType>(HttpResponseMessage? response) where MessageType : BaseMessage
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
