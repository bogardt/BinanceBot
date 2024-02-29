using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class CommissionRateValidatorTests
    {
        [TestMethod]
        public void CommissionRateValidatorShouldSuccess()
        {
            // Arrange
            var validator = new CommissionRateValidator();
            var commissionRate = ValidatorContext.Component.commissionRateValid;

            // Act
            var result = validator.Validate(commissionRate);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void CommissionRateValidatorWithInvalidTypeShouldFailed()
        {
            // Arrange
            var validator = new CommissionRateValidator();
            var commissionRate = ValidatorContext.Component.commissionRateUnvalidCharacters;

            // Act
            var result = validator.Validate(commissionRate);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CommissionRateValidatorBuyerWithNullValueShouldFailed()
        {
            // Arrange
            var validator = new CommissionRateValidator();
            var commissionRate = ValidatorContext.Component.commissionRateNullOrEmpty;

            // Act
            var result = validator.Validate(commissionRate);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
