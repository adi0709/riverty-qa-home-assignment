using CardValidation.Core.Enums;
using CardValidation.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Reqnroll;
using System.Net;
using System.Text;

namespace CardValidation.Tests.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CardValidationSteps
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private CreditCard _creditCard = new();
        private HttpResponseMessage? _response;
        private string? _responseContent;

        public CardValidationSteps()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Given(@"the card validation API is running")]
        public void GivenTheCardValidationApiIsRunning()
        {
            _client.Should().NotBeNull();
        }

        [Given(@"I have a credit card with the following details:")]
        public void GivenIHaveACreditCardWithTheFollowingDetails(Table table)
        {
            _creditCard = new CreditCard();
            
            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                var value = row["Value"];

                switch (field.ToLower())
                {
                    case "owner":
                        _creditCard.Owner = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "number":
                        _creditCard.Number = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "date":
                        _creditCard.Date = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "cvv":
                        _creditCard.Cvv = string.IsNullOrEmpty(value) ? null : value;
                        break;
                }
            }
        }

        [When(@"I submit the card for validation")]
        public async Task WhenISubmitTheCardForValidation()
        {
            var json = JsonConvert.SerializeObject(_creditCard);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _response = await _client.PostAsync("/CardValidation/card/credit/validate", content);
            _responseContent = await _response.Content.ReadAsStringAsync();
        }

        [Then(@"the response should be successful")]
        public void ThenTheResponseShouldBeSuccessful()
        {
            _response.Should().NotBeNull();
            _response!.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"the response should be a bad request")]
        public void ThenTheResponseShouldBeABadRequest()
        {
            _response.Should().NotBeNull();
            _response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Then(@"the payment system type should be ""(.*)""")]
        public void ThenThePaymentSystemTypeShouldBe(string expectedType)
        {
            _responseContent.Should().NotBeNullOrEmpty();
            
            var expectedEnumValue = Enum.Parse<PaymentSystemType>(expectedType);
            var actualEnumValue = JsonConvert.DeserializeObject<PaymentSystemType>(_responseContent!);
            
            actualEnumValue.Should().Be(expectedEnumValue);
        }

        [Then(@"the validation error should contain ""(.*)""")]
        public void ThenTheValidationErrorShouldContain(string expectedError)
        {
            _responseContent.Should().NotBeNullOrEmpty();
            _responseContent.Should().Contain(expectedError);
        }

        [AfterScenario]
        public void Cleanup()
        {
            _response?.Dispose();
        }

        [AfterFeature]
        public static void TearDown()
        {
        }
    }
}