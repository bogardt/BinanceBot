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

##Projects

BinanceBot                      # Project
├── Abstraction                 # Folder containing interface definitions            
├── Core                        # Folder containing core functionality like trading logic, price retrieval, and technical indicators
├── Model                       # Folder containing the models for accounts, balances, orders, etc.        
├── Strategy                    # Folder containing trading strategies 
├── Utils                       # Folder containing utility classes such as file system access, HTTP client wrapper, and logger
└──  Program.cs                 # Main entry point of the application

BinanceBot                    # Project
├── Abstraction               # Folder containing interfaces
│   ├── IBinanceClient.cs         # Interface for interacting with the Binance API
│   ├── IFileSystem.cs            # Interface for filesystem abstraction
│   ├── IHttpClientWrapper.cs     # Interface for HTTP call abstraction
│   ├── ILogger.cs                # Interface for logging abstraction
│   ├── IMarketTradeHandler.cs    # Interface for market trade handling
│   ├── IPriceRetriever.cs        # Interface for price retrieval
│   ├── ITechnicalIndicatorsCalculator.cs # Interface for technical indicators calculation
│   └── ITradeAction.cs           # Interface for trade action execution
├── Core                      # Folder containing core logic
│   ├── BinanceClient.cs          # Implementation for interacting with the Binance API
│   ├── MarketTradeHandler.cs     # Market trade handler
│   ├── PriceRetriever.cs         # Component for retrieving prices
│   ├── TechnicalIndicatorsCalculator.cs # Technical indicators calculator
│   └── TradeAction.cs            # Trade action executor
├── Model                     # Folder containing models
│   ├── Account.cs                # Model for user accounts
│   ├── Balance.cs                # Model for account balances
│   ├── Commission.cs             # Model for commissions
│   ├── CommissionRates.cs        # Model for commission rates
│   ├── Currency.cs               # Model for currencies
│   ├── Order.cs                  # Model for trade orders
│   └── TestOrder.cs              # Model for test orders
├── Strategy                  # Folder containing trading strategies
│   ├── StopLossStrategy.cs       # Stop loss strategy
│   └── TradingStrategy.cs        # Trading strategy
├── Utils                     # Folder containing utilities
│   ├── FileSystem.cs             # Utility for file management
│   ├── Helper.cs                 # Various helper methods
│   ├── HttpClientWrapper.cs      # Wrapper for HTTP calls
│   └── Logger.cs                 # Logging system
├── Program.cs                # File to configure the worker when running the project
└── Terraform                 # Configuration folder for infrastructure deployment (hypothetical)
