using BinanceBot.BinanceApi.Model;

namespace BinanceBot.Abstraction;

public interface IApiValidatorService
{
    Task<T> ValidateAsync<T>(HttpResponseMessage? response) where T : BaseMessage;
    //Task<MessageType> ValidateResponse<MessageType>(HttpResponseMessage? response);
    Task<IEnumerable<T>> Validate1DResponse<T>(HttpResponseMessage? response) where T : BaseMessage;
    Task<IEnumerable<IEnumerable<object>>> Validate2DMatriceResponse(HttpResponseMessage? response);
}