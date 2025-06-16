Feature: Credit Card Validation
    As a payment processor
    I want to validate credit card information
    So that I can ensure only valid cards are processed

Background:
    Given the card validation API is running

Scenario: Valid Visa card validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   | 12/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be successful
    And the payment system type should be "Visa"

Scenario: Valid MasterCard validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | Jane Smith          |
        | Number | 5555555555554444    |
        | Date   | 06/2026             |
        | Cvv    | 456                 |
    When I submit the card for validation
    Then the response should be successful
    And the payment system type should be "MasterCard"

Scenario: Valid American Express card validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | Bob Johnson         |
        | Number | 378282246310005     |
        | Date   | 03/2027             |
        | Cvv    | 7890                |
    When I submit the card for validation
    Then the response should be successful
    And the payment system type should be "AmericanExpress"

Scenario: Missing owner field validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  |                     |
        | Number | 4111111111111111    |
        | Date   | 12/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Owner is required"

Scenario: Missing card number validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number |                     |
        | Date   | 12/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Number is required"

Scenario: Missing expiry date validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   |                     |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Date is required"

Scenario: Missing CVV validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   | 12/2025             |
        | Cvv    |                     |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Cvv is required"

Scenario: Invalid card owner format
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John123 Doe         |
        | Number | 4111111111111111    |
        | Date   | 12/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Wrong owner"

Scenario: Invalid card number format
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 1234567890123456    |
        | Date   | 12/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Wrong number"

Scenario: Invalid expiry date format
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   | 13/2025             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Wrong date"

Scenario: Expired card validation
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   | 01/2020             |
        | Cvv    | 123                 |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Wrong date"

Scenario: Invalid CVV format
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  | John Doe            |
        | Number | 4111111111111111    |
        | Date   | 12/2025             |
        | Cvv    | 12                  |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Wrong cvv"

Scenario: Multiple validation errors
    Given I have a credit card with the following details:
        | Field  | Value               |
        | Owner  |                     |
        | Number |                     |
        | Date   |                     |
        | Cvv    |                     |
    When I submit the card for validation
    Then the response should be a bad request
    And the validation error should contain "Owner is required"
    And the validation error should contain "Number is required"
    And the validation error should contain "Date is required"
    And the validation error should contain "Cvv is required"

Scenario Outline: Multiple valid card types
    Given I have a credit card with the following details:
        | Field  | Value        |
        | Owner  | <owner>      |
        | Number | <number>     |
        | Date   | <date>       |
        | Cvv    | <cvv>        |
    When I submit the card for validation
    Then the response should be successful
    And the payment system type should be "<expected_type>"

    Examples:
        | owner      | number           | date    | cvv  | expected_type   |
        | Alice Doe  | 4000000000000002 | 01/2026 | 123  | Visa            |
        | Bob Smith  | 5105105105105100 | 02/2026 | 456  | MasterCard      |
        | Carol Lee  | 371449635398431  | 03/2026 | 7890 | AmericanExpress |