using BinanceBot.BinanceApi.Model.Message;

namespace BinanceBot.Abstraction;

public interface IApiValidator<MessageType> where MessageType : BaseMessage
{
    Task<MessageType> ValidateResponse(HttpResponseMessage? response);
    Task<IEnumerable<MessageType>> Validate1DResponse(HttpResponseMessage? response);
    Task<IEnumerable<IEnumerable<object>>> Validate2DMatriceResponse(HttpResponseMessage? response);
}