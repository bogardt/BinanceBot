using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class DiscountValidatorTests
    {
        [TestMethod]
        public void DiscountValidatorShouldSuccess()
        {
            // Arrange
            var validator = new DiscountValidator();
            var discount = ValidatorContext.Component.discountValid;

            // Act
            var result = validator.Validate(discount);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void DiscountValidatorWithoutBalanceAssetAndFreeShouldFailed()
        {
            // Arrange
            var validator = new DiscountValidator();
            var discount = ValidatorContext.Component.discountUnvalid;

            // Act
            var result = validator.Validate(discount);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
