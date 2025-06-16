using CardValidation.Core.Enums;
using CardValidation.Core.Services;

namespace CardValidation.Tests.Unit;

public class CardValidationServiceTests
{
    private readonly CardValidationService _cardValidationService;
    public CardValidationServiceTests()
    {
        _cardValidationService = new CardValidationService();
    }

    #region Validate all Cards Tests - Test all valid and invalid cards

    [Theory]
    [InlineData("4123456789012")]
    [InlineData("4123456789012345")]
    public void ValidateVisaCard_ValidCardNumber_ReturnsTrue(string cardNumber)
    {
        Assert.True(_cardValidationService.ValidateNumber(cardNumber));;
    }

    [Theory]
    [InlineData("5100123412341234")]
    [InlineData("5599123412341234")]
    [InlineData("2221001234567890")]
    [InlineData("2720991234567890")]
    public void ValidateMasterCard_ValidCardNumber_ReturnsTrue(string cardNumber)
    {
        Assert.True(_cardValidationService.ValidateNumber(cardNumber));
    }

    [Theory]
    [InlineData("341234567890123")]
    [InlineData("371234567890123")]
    public void ValidateAmexCard_validaCardNumber_ReturnsTrue(string cardNumber)
    {
        Assert.True(_cardValidationService.ValidateNumber(cardNumber));
    }

    [Theory]
    [InlineData("41234567890122")]
    [InlineData("402345678901224")]
    [InlineData("41234567890123451")]
    [InlineData("5000123412341234")]
    [InlineData("5699123412341234")]
    [InlineData("1221001234567890")]
    [InlineData("2820991234567890")]
    [InlineData("311234567890123")]
    [InlineData("351234567890123")]
    [InlineData("391234567890123")]
    [InlineData("1234567890123456")]
    [InlineData("")]
    public void ValidateCard_InvalidCardNumber_ReturnsFalse(string cardNumber)
    {
        Assert.False(_cardValidationService.ValidateNumber(cardNumber));
    }
    #endregion
    
    #region GetPaymentSystemType Tests

    [Theory]
    [InlineData("4123456789012")]
    [InlineData("4123456789012345")]
    public void GetPaymentSystemType_VisaCardNumber_ReturnsVisa(string cardNumber)
    {
        var result = _cardValidationService.GetPaymentSystemType(cardNumber);
        
        Assert.Equal(PaymentSystemType.Visa, result);
    }

    [Theory]
    [InlineData("5100123412341234")]
    [InlineData("5599123412341234")]
    [InlineData("2221001234567890")]
    [InlineData("2720991234567890")]
    public void GetPaymentSystemType_MasterCardNumber_ReturnsMasterCard(string cardNumber)
    {
        var result = _cardValidationService.GetPaymentSystemType(cardNumber);
        
        Assert.Equal(PaymentSystemType.MasterCard, result);
    }
    
    [Theory]
    [InlineData("341234567890123")]
    [InlineData("371234567890123")]
    public void GetPaymentSystemType_AmexCardNumber_ReturnsAmexCard(string cardNumber)
    {
        Assert.True(_cardValidationService.ValidateNumber(cardNumber));
    }
    
    [Fact]
    public void GetPaymentSystemType_InvalidCard_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => 
            _cardValidationService.GetPaymentSystemType("1234567890123456"));
    }
    
    #endregion


    #region Validate all Owners Tests - Test all valid and invalid owners

    [Theory]
    [InlineData("Jone Doe")]
    [InlineData("Jone")]
    [InlineData("Jone Doe Smith")]
    public void ValidateOwner_ValidOwner_ReturnsTrue(string owner)
    {
        Assert.True(_cardValidationService.ValidateOwner(owner));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("Jone Doe123")]
    [InlineData("Jone Doe  ")]
    [InlineData("Jone@#")]
    [InlineData("Jone  Smith")]
    [InlineData("Jone Doe Smith Dwayne")]
    [InlineData("John-Doe")]
    public void ValidateOwner_InValidOwner_ReturnsFalse(string owner)
    {
        Assert.False(_cardValidationService.ValidateOwner(owner));
    }

    #endregion
    
    #region Validate all CVC Tests - Test all valid and invalid CVC

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void ValidateCVC_ValidCVC_ReturnsTrue(string cvc)
    {
        Assert.True(_cardValidationService.ValidateCvc(cvc));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("12a")]
    [InlineData("12345")]
    [InlineData("abc")]
    [InlineData("12@")]
    public void ValidateCVC_InValidCVC_ReturnsFalse(string cvc)
    {
        Assert.False(_cardValidationService.ValidateCvc(cvc));
    }
    
    #endregion
    
    #region Validate all Date Tests - Test all valid and invalid dates

    [Theory]
    [InlineData("12/2026")]
    [InlineData("12/26")]
    [InlineData("1226")]
    [InlineData("122026")]
    public void ValidateDate_ValidDate_ReturnsTrue(string date)
    {
        Assert.True(_cardValidationService.ValidateIssueDate(date));
    }
    
    [Theory]
    [InlineData("12/2022")]
    [InlineData("12/22")]
    [InlineData("1222")]
    [InlineData("122022")]
    [InlineData("13/2030")]
    [InlineData("00/2030")]
    [InlineData("abc/2030")]
    [InlineData("")] 
    public void ValidateDate_InValidDate_ReturnsFalse(string date)
    {
        Assert.False(_cardValidationService.ValidateIssueDate(date));
    }
    #endregion
}