using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class BalanceValidatorTests
    {
        [TestMethod]
        public void BalanceValidatorShouldSuccess()
        {
            // Arrange
            var validator = new BalanceValidator();
            var balance = ValidatorContext.Component.balanceValid;

            // Act
            var result = validator.Validate(balance);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void BalanceValidatorEmptyShouldFailed()
        {
            // Arrange
            var validator = new BalanceValidator();
            var balance = ValidatorContext.Component.balanceEmpty;

            // Act
            var result = validator.Validate(balance);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
