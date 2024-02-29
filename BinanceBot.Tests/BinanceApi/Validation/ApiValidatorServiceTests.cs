using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Newtonsoft.Json;

namespace BinanceBot.Tests.BinanceApi.Validation
{
    [TestClass]
    public class ApiValidatorServiceTests
    {
        [TestMethod]
        public async Task ValidateAsyncWithValidCommissionShouldSuccessValidation()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var validatorMock = new Mock<IValidator<Commission>>();

            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<Commission>>(), default))
                         .ReturnsAsync(new ValidationResult());

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Commission>)))
                               .Returns(validatorMock.Object);

            var apiValidatorService = new ApiValidatorService(serviceProviderMock.Object);
            var commission = new Commission
            {
                Discount = Context.ValidatorContext.Component.discountValid,
                StandardCommission = Context.ValidatorContext.Component.commissionRateValid,
                TaxCommission = Context.ValidatorContext.Component.commissionRateValid
            };
            var json = JsonConvert.SerializeObject(commission);

            // Act
            var result = await apiValidatorService.ValidateAsync<Commission>(json);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}