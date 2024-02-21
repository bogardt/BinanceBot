# Bot Crypto Binance
---
[![.NET Coverage and GitHub Pages](https://github.com/bogardt/BinanceBot/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/bogardt/BinanceBot/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/github/bogardt/BinanceBot/graph/badge.svg?token=V2F65ESN7M)](https://codecov.io/github/bogardt/BinanceBot)

### Code Coverage Report
Check out our [code coverage report](https://codecov.io/github/bogardt/BinanceBot) for more details.
![Code Coverage Graph](https://codecov.io/github/bogardt/BinanceBot/graphs/sunburst.svg?token=V2F65ESN7M)

## Getting Started
---
### Prerequisites
* Visual Studio 2022
* .NET 8

### Start the bot
* Clone the project
* Open the solution with Visual Studio
* Build the solution
* Run the project

## Running the tests
* Open Test Explorer
* Run all tests

### Built With
* [Visual Studio 2022](https://visualstudio.microsoft.com/fr/downloads/) - The development IDE
* [.Net Core 3.1](https://www.microsoft.com/net/download/dotnet-core/3.1) - The framework

### Configuration settings
* Create a file appsettings.json in the project root solution folder with your binance api key and secret
```json
{
  "AppSettings": {
    "Binance": {
      "ApiKey": "***",
      "ApiSecret": "***"
    }
    },
  "ConnectionStrings": {
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Project
BinanceBot                      # Project<br>
├── Abstraction                 # Folder containing interface definitions<br>
├── Core                        # Folder containing core functionality like trading logic, price retrieval, and technical indicators<br>
├── Model                       # Folder containing the models for accounts, balances, orders, etc.<br>
├── Strategy                    # Folder containing trading strategies<br>
├── Utils                       # Folder containing utility classes such as file system access, HTTP client wrapper, and logger<br>
└──  Program.cs                 # Main entry point of the application<br>

### Detailed project
BinanceBot # Project Name<br>
├── Abstraction # Folder containing interfaces<br>
│ ├── IBinanceClient.cs # Interface for interacting with the Binance API<br>
│ ├── IFileSystem.cs # Interface for filesystem abstraction<br>
│ ├── IHttpClientWrapper.cs # Interface for HTTP call abstraction<br>
│ ├── ILogger.cs # Interface for logging abstraction<br>
│ ├── IMarketTradeHandler.cs # Interface for market trade handling<br>
│ ├── IPriceRetriever.cs # Interface for price retrieval<br>
│ ├── ITechnicalIndicatorsCalculator.cs # Interface for technical indicators calculation<br>
│ └── ITradeAction.cs # Interface for trade action execution<br>
├── Core # Folder containing core logic<br>
│ ├── BinanceClient.cs # Implementation for interacting with the Binance API<br>
│ ├── MarketTradeHandler.cs # Market trade handler<br>
│ ├── PriceRetriever.cs # Component for retrieving prices<br>
│ ├── TechnicalIndicatorsCalculator.cs # Technical indicators calculator<br>
│ └── TradeAction.cs # Trade action executor<br>
├── Model # Folder containing models<br>
│ ├── Account.cs # Model for user accounts<br>
│ ├── Balance.cs # Model for account balances<br>
│ ├── Commission.cs # Model for commissions<br>
│ ├── CommissionRates.cs # Model for commission rates<br>
│ ├── Currency.cs # Model for currencies<br>
│ ├── Order.cs # Model for trade orders<br>
│ └── TestOrder.cs # Model for test orders<br>
├── Strategy # Folder containing trading strategies<br>
│ ├── StopLossStrategy.cs # Stop loss strategy<br>
│ └── TradingStrategy.cs # Trading strategy<br>
├── Utils # Folder containing utilities<br>
│ ├── FileSystem.cs # Utility for file management<br>
│ ├── Helper.cs # Various helper methods<br>
│ ├── HttpClientWrapper.cs # Wrapper for HTTP calls<br>
│ └── Logger.cs # Logging system<br>
└── Program.cs # File to configure the worker when running the project<br>
