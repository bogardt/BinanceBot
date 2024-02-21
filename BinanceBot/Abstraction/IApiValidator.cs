using BinanceBot.BinanceApi.Model.Message;

namespace BinanceBot.Abstraction;

public interface IApiValidator
{
    Task<MessageType> ValidateResponse<MessageType>(HttpResponseMessage? response) where MessageType : BaseMessage;
    Task<IEnumerable<MessageType>> Validate1DResponse<MessageType>(HttpResponseMessage? response) where MessageType : BaseMessage;
    Task<IEnumerable<IEnumerable<object>>> Validate2DMatriceResponse(HttpResponseMessage? response);
}