using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class TestOrderValidatorTests
    {
        [TestMethod]
        public void TestOrderValidatorShouldSuccess()
        {
            // Arrange
            var validator = new TestOrderValidator();
            var commission = new TestOrder
            {
                Discount = ValidatorContext.Component.discountValid,
                StandardCommissionForOrder = ValidatorContext.Component.commissionRateValid,
                TaxCommissionForOrder = ValidatorContext.Component.commissionRateValid
            };

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void TestOrderValidatorWithNullOrEmptyValuesShouldFailed()
        {
            // Arrange
            var validator = new TestOrderValidator();
            var commission = new TestOrder
            {
                Discount = ValidatorContext.Component.discountUnvalid,
                StandardCommissionForOrder = ValidatorContext.Component.commissionRateNullOrEmpty,
                TaxCommissionForOrder = ValidatorContext.Component.commissionRateValid
            };

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void TestOrderValidatorWithEmptyOrNullValuesShouldFailed()
        {
            // Arrange
            var validator = new TestOrderValidator();
            var commission = new TestOrder
            {
                Discount = ValidatorContext.Component.discountUnvalid,
                StandardCommissionForOrder = ValidatorContext.Component.commissionRateValid,
                TaxCommissionForOrder = ValidatorContext.Component.commissionRateNullOrEmpty
            };

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void TestOrderValidatorErrorCodeAndMessage()
        {
            // Arrange
            var validator = new TestOrderValidator();
            var commission = ValidatorContext.ErrorMessage<TestOrder>();

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
