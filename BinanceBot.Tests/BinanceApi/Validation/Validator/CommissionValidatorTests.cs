using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class CommissionValidatorTests
    {
        [TestMethod]
        public void CommissionValidatorShouldSuccess()
        {
            // Arrange
            var validator = new CommissionValidator();
            var commission = ValidatorContext.commissionValid;

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void CommissionValidatorWithNullOrEmptyValuesShouldFailed()
        {
            // Arrange
            var validator = new CommissionValidator();
            var commission = ValidatorContext.commissionValidWithStdValidAndTaxNull;

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CommissionValidatorWithEmptyOrNullValuesShouldFailed()
        {
            // Arrange
            var validator = new CommissionValidator();
            var commission = ValidatorContext.commissionUnvalidWithStdNullAndTaxValid;

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void CommissionValidatorErrorCodeAndMessage()
        {
            // Arrange
            var validator = new CommissionValidator();
            var commission = ValidatorContext.ErrorMessage<Commission>();

            // Act
            var result = validator.Validate(commission);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
