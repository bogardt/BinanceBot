using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation.Validator;
using BinanceBot.Tests.BinanceApi.Validation.Context;

namespace BinanceBot.Tests.BinanceApi.Validation.Validator
{
    [TestClass]
    public class OrderValidatorTests
    {
        [TestMethod]
        public void OrderValidatorShouldSuccess()
        {
            // Arrange
            var validator = new OrderValidator();
            var order = ValidatorContext.Component.orderValid;

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void OrderValidatorShouldFailed()
        {
            // Arrange
            var validator = new OrderValidator();
            var order = ValidatorContext.Component.orderUnvalid;

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void OrderValidatorErrorCodeAndMessage()
        {
            // Arrange
            var validator = new OrderValidator();
            var order = ValidatorContext.ErrorMessage<Order>();

            // Act
            var result = validator.Validate(order);

            // Assert
            Assert.IsFalse(result.IsValid);
        }
    }
}
