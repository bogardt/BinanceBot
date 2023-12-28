# Bot Crypto Binance
---
[![.NET Coverage and GitHub Pages](https://github.com/bogardt/BinanceBot/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/bogardt/BinanceBot/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/github/bogardt/BinanceBot/graph/badge.svg?token=V2F65ESN7M)](https://codecov.io/github/bogardt/BinanceBot)

### Code Coverage Report
Check out our [code coverage report](https://bogardt.github.io/BinanceBot/index.html) for more details.
![Code Coverage Graph](https://codecov.io/github/bogardt/BinanceBot/graphs/sunburst.svg?token=V2F65ESN7M)

## Getting Started
---
### Prerequisites
* Visual Studio 2022
* .Net Core 3.1

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