namespace BinanceBot.BinanceApi.Validation;

public record ValidationError(string PropertyName, string ErrorMessage);
