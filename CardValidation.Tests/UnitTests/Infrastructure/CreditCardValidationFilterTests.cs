using CardValidation.Core.Services.Interfaces;
using CardValidation.Infrustructure;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CardValidation.Tests.Infrastructure
{
    public class CreditCardValidationFilterTests
    {
        private readonly Mock<ICardValidationService> _mockCardValidationService;
        private readonly CreditCardValidationFilter _creditCardValidationFilter;

        public CreditCardValidationFilterTests()
        {
            _mockCardValidationService = new Mock<ICardValidationService>();
            _creditCardValidationFilter = new CreditCardValidationFilter(_mockCardValidationService.Object);
        }

        [Fact]
        public void Constructor_WithValidService_ShouldCreateInstance()
        {
            var filter = new CreditCardValidationFilter(_mockCardValidationService.Object);
            
            Assert.NotNull(filter);
        }

        [Fact]
        public void OnActionExecuting_WithNullActionArguments_ShouldNotThrow()
        {
            var context = CreateActionExecutingContext(null);
            
            var exception = Record.Exception(() => _creditCardValidationFilter.OnActionExecuting(context));
            Assert.Null(exception);
        }

        [Fact]
        public void OnActionExecuting_WithEmptyActionArguments_ShouldNotThrow()
        {
            var context = CreateActionExecutingContext(new Dictionary<string, object?>());
            
            var exception = Record.Exception(() => _creditCardValidationFilter.OnActionExecuting(context));
            Assert.Null(exception);
        }

        [Fact]
        public void OnActionExecuting_WithoutCreditCardArgument_ShouldNotThrow()
        {
            var actionArguments = new Dictionary<string, object?>
            {
                { "otherParam", "value" }
            };
            var context = CreateActionExecutingContext(actionArguments);
            
            var exception = Record.Exception(() => _creditCardValidationFilter.OnActionExecuting(context));
            Assert.Null(exception);
        }

        [Fact]
        public void OnActionExecuting_WithNullCreditCard_ShouldThrowInvalidOperationException()
        {
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", null }
            };
            var context = CreateActionExecutingContext(actionArguments);
            
            Assert.Throws<InvalidOperationException>(() => _creditCardValidationFilter.OnActionExecuting(context));
        }

        [Fact]
        public void OnActionExecuting_WithInvalidCreditCardType_ShouldThrowInvalidOperationException()
        {
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", "not a credit card object" }
            };
            var context = CreateActionExecutingContext(actionArguments);
            
            Assert.Throws<InvalidOperationException>(() => _creditCardValidationFilter.OnActionExecuting(context));
        }

        [Fact]
        public void OnActionExecuting_WithValidCreditCard_AllFieldsValid_ShouldNotAddErrors()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.True(context.ModelState.IsValid);
            Assert.Empty(context.ModelState);
        }

        [Fact]
        public void OnActionExecuting_WithNullOwner_ShouldAddRequiredError()
        {
            var creditCard = new CreditCard
            {
                Owner = null,
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Owner"));
            Assert.Equal("Owner is required", context.ModelState["Owner"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithEmptyOwner_ShouldAddRequiredError()
        {
            var creditCard = new CreditCard
            {
                Owner = string.Empty,
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Owner"));
            Assert.Equal("Owner is required", context.ModelState["Owner"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithInvalidOwner_ShouldAddWrongParameterError()
        {
            var creditCard = new CreditCard
            {
                Owner = "Invalid123",
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("Invalid123")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Owner"));
            Assert.Equal("Wrong owner", context.ModelState["Owner"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithNullNumber_ShouldAddRequiredError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = null,
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Number"));
            Assert.Equal("Number is required", context.ModelState["Number"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithInvalidNumber_ShouldAddWrongParameterError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "1234567890123456",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("1234567890123456")).Returns(false);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Number"));
            Assert.Equal("Wrong number", context.ModelState["Number"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithNullDate_ShouldAddRequiredError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = null,
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Date"));
            Assert.Equal("Date is required", context.ModelState["Date"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithInvalidDate_ShouldAddWrongParameterError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = "13/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("13/2025")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Date"));
            Assert.Equal("Wrong date", context.ModelState["Date"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithNullCvv_ShouldAddRequiredError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = null
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Cvv"));
            Assert.Equal("Cvv is required", context.ModelState["Cvv"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithInvalidCvv_ShouldAddWrongParameterError()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "12"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/2025")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("12")).Returns(false);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Cvv"));
            Assert.Equal("Wrong cvv", context.ModelState["Cvv"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuting_WithMultipleInvalidFields_ShouldAddMultipleErrors()
        {
            var creditCard = new CreditCard
            {
                Owner = null,
                Number = "invalid",
                Date = "13/2025",
                Cvv = "12"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("13/2025")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateNumber("invalid")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateCvc("12")).Returns(false);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            Assert.False(context.ModelState.IsValid);
            Assert.True(context.ModelState.ContainsKey("Owner"));
            Assert.True(context.ModelState.ContainsKey("Number"));
            Assert.True(context.ModelState.ContainsKey("Date"));
            Assert.True(context.ModelState.ContainsKey("Cvv"));
            Assert.Equal("Owner is required", context.ModelState["Owner"]!.Errors[0].ErrorMessage);
            Assert.Equal("Wrong number", context.ModelState["Number"]!.Errors[0].ErrorMessage);
            Assert.Equal("Wrong date", context.ModelState["Date"]!.Errors[0].ErrorMessage);
            Assert.Equal("Wrong cvv", context.ModelState["Cvv"]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public void OnActionExecuted_ShouldNotThrow()
        {
            var context = CreateActionExecutedContext();
            
            var exception = Record.Exception(() => _creditCardValidationFilter.OnActionExecuted(context));
            Assert.Null(exception);
        }

        [Fact]
        public void OnActionExecuting_ShouldCallAllValidationMethods()
        {
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Number = "4111111111111111",
                Date = "12/2025",
                Cvv = "123"
            };
            
            var actionArguments = new Dictionary<string, object?>
            {
                { "creditCard", creditCard }
            };
            var context = CreateActionExecutingContext(actionArguments);

            _mockCardValidationService.Setup(x => x.ValidateOwner(It.IsAny<string>())).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate(It.IsAny<string>())).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc(It.IsAny<string>())).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber(It.IsAny<string>())).Returns(true);
            
            _creditCardValidationFilter.OnActionExecuting(context);
            
            _mockCardValidationService.Verify(x => x.ValidateOwner("John Doe"), Times.Once);
            _mockCardValidationService.Verify(x => x.ValidateIssueDate("12/2025"), Times.Once);
            _mockCardValidationService.Verify(x => x.ValidateCvc("123"), Times.Once);
            _mockCardValidationService.Verify(x => x.ValidateNumber("4111111111111111"), Times.Once);
        }

        private ActionExecutingContext CreateActionExecutingContext(Dictionary<string, object?>? actionArguments = null)
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            
            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments ?? new Dictionary<string, object?>(),
                controller: null!);
        }

        private ActionExecutedContext CreateActionExecutedContext()
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            
            return new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!);
        }
    }
}
