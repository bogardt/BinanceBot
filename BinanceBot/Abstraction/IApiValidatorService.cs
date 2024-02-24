using BinanceBot.BinanceApi.Model;

namespace BinanceBot.Abstraction;

public interface IApiValidatorService
{
    Task<T> ValidateAsync<T>(string json) where T : IMessage;
    Task<IMessage> ValidationType(IMessage message, string json);
}