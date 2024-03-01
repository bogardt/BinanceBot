using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class AccountValidatorTests
    {
        [TestMethod]
        public void AccountValidatorShouldSuccess()
        {
            // Arrange
            var validator = new AccountValidator();
            var account = ValidatorContext.accountValid;

            // Act
            var result = validator.Validate(account);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void AccountValidatorWithoutBalanceAssetAndFreeShouldFailed()
        {
            // Arrange
            var validator = new AccountValidator();
            var account = ValidatorContext.accountBalanceEmpty;
            // Act
            var result = validator.Validate(account);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void AccountValidatorShouldFailed()
        {
            // Arrange
            var validator = new AccountValidator();
            var account = ValidatorContext.ErrorMessage<Account>();

            // Act
            var result = validator.Validate(account);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
