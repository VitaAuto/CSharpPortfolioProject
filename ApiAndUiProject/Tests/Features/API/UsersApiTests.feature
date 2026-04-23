@api
Feature: Users API tests

  Scenario Outline: Get user by id
    Given user is logged in
    And I have user with first name "Sam", last name "Lamour", email "sam@mail.com", is active true
    And user email "sam@mail.com" is unique
    When I send POST request to create user
    Then response status should be 201
    When I send GET request to get user by id
    Then response status should be 200
  
  @smoke
  Scenario Outline: Create user
    Given user is logged in
    And I have user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And user email "<Email>" is unique
    When I send POST request to create user
    Then response status should be <Status>
    And response should contain "<ExpectedText>"

    Examples:
      | FirstName | LastName | Email            | IsActive | Status | ExpectedText                                |
      | Ivan      | Ivanov   | ivan@mail.com    | true     | 201    | Ivanov                                      |
      | Petr      | Petrov   | petr@mail.com    | false    | 201    | Petrov                                      |
      |           | Larson   | larson@mail.com  | true     | 400    | FirstName is required                       |
      | Joe       | Konrad   | invalidemail     | true     | 400    | Email is not valid                          |

  Scenario Outline: Create user with duplicate email
    Given user is logged in
    And I have user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And I have another user with first name "<OtherFirstName>", last name "<OtherLastName>", email "<Email>", is active <OtherIsActive>
    When I send POST request to create user
    And I send POST request to create another user
    Then response status should be 409
    And response should contain "User with this email already exists."

    Examples:
      | FirstName | LastName | Email         | IsActive | OtherFirstName | OtherLastName | OtherIsActive |
      | Lola      | Ivanova  | lola@mail.com | true     | Henry          | Muler         | false         |

  @smoke
  Scenario Outline: Update user with PUT
    Given user is logged in
    And I have user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And user email "<Email>" is unique
    When I send POST request to create user
    Then user email "<NewEmail>" is unique
    When I send PUT request to update user with first name "<NewFirstName>", last name "<NewLastName>", email "<NewEmail>", is active <NewIsActive>
    Then response status should be <Status>
    And response should contain "<ExpectedText>"

    Examples:
    | FirstName | LastName | Email           | IsActive | NewFirstName | NewLastName | NewEmail           | NewIsActive | Status | ExpectedText           |
    | Kevin     | Lost     | kevin@mail.com  | true     | Updated      | User        | updated@mail.com   | true        | 200    | Updated                |
    | Bob       | Brown    | bob@mail.com    | true     |              | Brown       | bob@mail.com       | false       | 400    | FirstName is required  |
    | Carol     | White    | carol@mail.com  | true     | Test         | Test        | invalidemail       | true        | 400    | Email is not valid     |

  @smoke
  Scenario Outline: Patch user with email
    Given user is logged in
    And I have user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    And user email "<Email>" is unique
    And I have another user with first name "<OtherFirstName>", last name "<OtherLastName>", email "<OtherEmail>", is active <OtherIsActive>
    And user email "<OtherEmail>" is unique
    When I send POST request to create user
    And I send POST request to create another user
    And I send PATCH request to update user with email "<PatchEmail>"
    Then response status should be <Status>
    And response should contain "<ExpectedText>"

    Examples:
      | FirstName  | LastName  | Email          | IsActive | OtherFirstName | OtherLastName  | OtherEmail       | OtherIsActive | PatchEmail      | Status | ExpectedText                                 |
      | Sofia      | Rudova    | sofia@mail.com | true     | Kui            | Serdov         | kui@mail.com     | true          | invalidemail    | 400    | Email is not valid                           |
     

  @smoke
  Scenario Outline: Delete user
    Given user is logged in
    And I have user with first name "<FirstName>", last name "<LastName>", email "<Email>", is active <IsActive>
    When I send POST request to create user
    And I send DELETE request to delete user
    Then response status should be 204
    When I send GET request to get user by id
    Then response status should be 404

    Examples:
      | FirstName | LastName | Email           | IsActive |
      | Ivanka    | Ivanova  | ivanka@mail.com | true     |
      | Petra     | Petrova  | petra@mail.com  | false    |

  Scenario: Delete user by non-existing id
    Given user is logged in
    When I send DELETE request to delete user by id 99999
    Then response status should be 404