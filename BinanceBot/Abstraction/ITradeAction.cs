﻿using BinanceBot.Strategy;

namespace BinanceBot.Abstraction
{
    public interface ITradeAction
    {
        Task Buy(TradingStrategy tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol);
        Task<(decimal, bool)> Sell(TradingStrategy tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol);
        Task WaitBuyAsync(string symbol);
        Task WaitSellAsync(string symbol);
    }
}