using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class CurrencyValidatorTests
    {
        [TestMethod]
        public void CurrencyValidatorShouldSuccess()
        {
            // Arrange
            var validator = new CurrencyValidator();
            var currency = ValidatorContext.Component.currencyValid;

            // Act
            var result = validator.Validate(currency);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void CurrencyValidatorPriceAndSymbolAreNullShouldFailed()
        {
            // Arrange
            var validator = new CurrencyValidator();
            var currency = ValidatorContext.Component.currencyUnvalid;

            // Act
            var result = validator.Validate(currency);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CurrencyValidatorErrorCodeAndMessage()
        {
            // Arrange
            var validator = new CurrencyValidator();
            var currency = ValidatorContext.ErrorMessage<Currency>();

            // Act
            var result = validator.Validate(currency);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
