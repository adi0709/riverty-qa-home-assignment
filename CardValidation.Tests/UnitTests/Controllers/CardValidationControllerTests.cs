using CardValidation.Controllers;
using CardValidation.Core.Enums;
using CardValidation.Core.Services.Interfaces;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CardValidation.Tests.Unit.Controllers;

public class CardValidationControllerTests
{
    private readonly Mock<ICardValidationService> _mockCardValidationService;
    private readonly CardValidationController _cardValidationController;

    public CardValidationControllerTests()
    {
        _mockCardValidationService = new Mock<ICardValidationService>();
        _cardValidationController = new CardValidationController(_mockCardValidationService.Object);
    }

    [Fact]
    public void ValidateVisaCard_ValidCard_ReturnsOkWithPaymentType()
    {
        var validVisaCard = new CreditCard
        {
            Owner = "John Doe",
            Number = "4111111111111111",
            Date = "12/2025",
            Cvv = "123"
        };

        _mockCardValidationService.Setup(service =>
            service.GetPaymentSystemType(It.IsRegex("^4[0-9]{12}(?:[0-9]{3})?$")))
            .Returns(PaymentSystemType.Visa);

        var result = _cardValidationController.ValidateCreditCard(validVisaCard);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(PaymentSystemType.Visa, okResult.Value);
        _mockCardValidationService.Verify(service => service.GetPaymentSystemType(validVisaCard.Number), Times.Once);
    }
    
    [Fact]
    public void ValidateMasterCard_ValidCard_ReturnsOkWithPaymentType()
    {
        var validMasterCard = new CreditCard
        {
            Owner = "John Doe",
            Number = "5555555555554444",
            Date = "12/2025",
            Cvv = "123"
        };
        
        _mockCardValidationService.Setup(service =>
            service.GetPaymentSystemType(It.IsRegex("^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$")))
            .Returns(PaymentSystemType.MasterCard);
        var result = _cardValidationController.ValidateCreditCard(validMasterCard);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(PaymentSystemType.MasterCard, okResult.Value);
        _mockCardValidationService.Verify(service => service.GetPaymentSystemType(validMasterCard.Number), Times.Once);
    }

    [Fact]
    public void ValidateAmexCard_ValidCard_ReturnsOkWithPaymentType()
    {
        var validAmexCard = new CreditCard
        {
            Owner = "John Doe",
            Number = "378282246310005",
            Date = "12/2025",
            Cvv = "123"
        };

        _mockCardValidationService.Setup(service => service.GetPaymentSystemType(It.IsRegex("^3[47][0-9]{13}$")))
            .Returns(PaymentSystemType.AmericanExpress);
        
        var result = _cardValidationController.ValidateCreditCard(validAmexCard);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(PaymentSystemType.AmericanExpress, okResult.Value);
        _mockCardValidationService.Verify(service => service.GetPaymentSystemType(validAmexCard.Number), Times.Once);
    }

    [Fact]
    public void ValidateCreditCard_ServiceThrowsException_ExceptionPropagates()
    {
        var invalidCard = new CreditCard
        {
            Owner = "Test User",
            Number = "1234567890123456",
            Date = "12/2025",
            Cvv = "123"
        };
        _mockCardValidationService.Setup(service => service.GetPaymentSystemType(invalidCard.Number))
            .Throws<NotImplementedException>();
        
        Assert.Throws<NotImplementedException>(() => 
            _cardValidationController.ValidateCreditCard(invalidCard));
    }
    
    
    [Fact]
    public void ValidateCreditCard_InvalidModal_ReturnsBadRequest()
    {
        var cardWithNullNumber = new CreditCard
        {
            Owner = "Test User",
            Number = "",
            Date = "12/2025",
            Cvv = "123"
        };
        
        _cardValidationController.ModelState.AddModelError("Number", "Number is required");
        
        var result = _cardValidationController.ValidateCreditCard(cardWithNullNumber);

        Assert.IsType<BadRequestObjectResult>(result);
        _mockCardValidationService.Verify(
            service => service.GetPaymentSystemType(It.IsAny<string>()), 
            Times.Never);
    }
    
    [Fact]
    public void ValidateCreditCard_NullCardNumber_CallsServiceWithEmptyString()
    {
        var cardWithNullNumber = new CreditCard
        {
            Owner = "Test User",
            Number = null,
            Date = "12/2025",
            Cvv = "123"
        };
        
        _mockCardValidationService
            .Setup(service => service.GetPaymentSystemType(string.Empty))
            .Returns(PaymentSystemType.Visa);
        
        var result = _cardValidationController.ValidateCreditCard(cardWithNullNumber);
        
        _mockCardValidationService.Verify(
            service => service.GetPaymentSystemType(string.Empty), 
            Times.Once);
    }
    
}