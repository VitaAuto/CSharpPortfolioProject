@api
Feature: Users API

  Scenario Outline: Get user by id
    Given user is logged in
    And I have a user with first name "Sam", last name "Lamour", email "sam@mail.com", is active true
    When I send a POST request to create the user
    Then the response status should be 201
    When I send a GET request to get the user by id
    Then the response status should be 200
  
  @smoke
  Scenario Outline: Create user
    Given user is logged in
    And I have a user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    When I send a POST request to create the user
    Then the response status should be <Status>
    And the response should contain "<ExpectedText>"

    Examples:
      | FirstName | LastName | Email         | IsActive | Status | ExpectedText                                |
      | Ivan      | Ivanov   | ivan@mail.com | true     | 201    | Ivanov                                      |
      | Petr      | Petrov   | petr@mail.com | false    | 201    | Petrov                                      |
      |           | Ivanov   | ivan@mail.com | true     | 400    | FirstName is required                       |
      | Ivan      | Ivanov   | invalidemail  | true     | 400    | Email is not valid                          |

  Scenario Outline: Create user with duplicate email
    Given user is logged in
    And I have a user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And I have another user with first name "<OtherFirstName>", last name "<OtherLastName>", email "<Email>", is active <OtherIsActive>
    When I send a POST request to create the user
    And I send a POST request to create the other user
    Then the response status should be 409
    And the response should contain "User with this email already exists."

    Examples:
      | FirstName | LastName | Email         | IsActive | OtherFirstName | OtherLastName | OtherIsActive |
      | Ivan      | Ivanov   | ivan@mail.com | true     | Petr           | Petrov        | false         |

  @smoke
  Scenario Outline: Update user with PUT
    Given user is logged in
    And I have a user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    When I send a POST request to create the user
    And I send a PUT request to update the user with first name "<NewFirstName>", last name "<NewLastName>", email "<NewEmail>", is active <NewIsActive>
    Then the response status should be <Status>
    And the response should contain "<ExpectedText>"

    Examples:
    | FirstName | LastName | Email           | IsActive | NewFirstName | NewLastName | NewEmail           | NewIsActive | Status | ExpectedText           |
    | Kevin     | Lost     | kevin@mail.com  | true     | Updated      | User        | updated@mail.com   | false       | 200    | Updated                |
    | Bob       | Brown    | bob@mail.com    | true     |              | Brown       | bob@mail.com       | true        | 400    | FirstName is required  |
    | Carol     | White    | carol@mail.com  | true     | Test         | Test        | invalidemail       | true        | 400    | Email is not valid     |

  @smoke
  Scenario Outline: Patch user with email
    Given user is logged in
    And I have a user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And I have another user with first name "<OtherFirstName>", last name "<OtherLastName>", email "<OtherEmail>", is active <OtherIsActive>
    When I send a POST request to create the user
    And I send a POST request to create the other user
    And I send a PATCH request to update the user with email "<PatchEmail>"
    Then the response status should be <Status>
    And the response should contain "<ExpectedText>"

    Examples:
      | FirstName  | LastName  | Email          | IsActive | OtherFirstName | OtherLastName  | OtherEmail       | OtherIsActive | PatchEmail      | Status | ExpectedText                                 |
      | Ivan1      | Ivanov1   | ivan1@mail.com | true     | Petr1          | Petrov1        | other1@mail.com  | false         | invalidemail    | 400    | Email is not valid                           |
     

  @smoke
  Scenario Outline: Delete user
    Given user is logged in
    And I have a user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    When I send a POST request to create the user
    And I send a DELETE request to delete the user
    Then the response status should be 204
    When I send a GET request to get the user by id
    Then the response status should be 404

    Examples:
      | FirstName | LastName | Email           | IsActive |
      | Ivanka    | Ivanova  | ivanka@mail.com | true     |
      | Petra     | Petrova  | petra@mail.com  | false    |

  Scenario: Delete user by non-existing id
    Given user is logged in
    When I send a DELETE request to delete the user by id 99999
    Then the response status should be 404