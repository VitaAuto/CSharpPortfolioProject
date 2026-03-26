using FluentAssertions;
using Newtonsoft.Json;
using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Auth;
using PlaywrightProject.API.Context;
using PlaywrightProject.API.Models;
using PlaywrightProject.API.Services;
using PlaywrightProject.Config;
using Reqnroll;
using System.Net;

namespace PlaywrightProject.Steps
{
    [Binding]
    public class UsersApiSteps
    {
        private readonly ApiContext _context;
        private readonly UserService _userService;
        private readonly UsersApiClient _usersApiClient;
        private readonly ITokenProvider _tokenProvider;

        public UsersApiSteps(ApiContext context, UserService userService, UsersApiClient usersApiClient, ITokenProvider tokenProvider)
        {
            _context = context;
            _userService = userService;
            _usersApiClient = usersApiClient;
            _tokenProvider = tokenProvider;
        }

        [Given(@"user is logged in")]
        public async Task GivenTheUserIsLoggedIn()
        {
            var vaultApiClient = new VaultApiClient(ApiConfig.VaultUri, ApiConfig.VaultToken);
            var (username, password) = await vaultApiClient.GetCredentialsAsync();

            var authApiClient = new AuthApiClient(ApiConfig.ApiBaseUrl);
            var token = authApiClient.GetToken(username, password);

            _tokenProvider.SetToken(token);
        }

        [Given(@"the user email ""(.*)"" is unique")]
        [Then(@"the user email ""(.*)"" is unique")]
        public void UserEmailIsUnique(string email)
        {
            _userService.EnsureUserEmailIsUnique(email);
        }

        [Given(@"I have a user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void GivenIHaveUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            _context.User = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };
        }

        [Given(@"I have another user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void GivenIHaveAnotherUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            _context.OtherUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };
        }

        [When(@"I send a POST request to create the user")]
        public void WhenISendPOSTRequestToCreateUser()
        {
            var response = _usersApiClient.CreateUser(_context.User);
            _context.Response = response;
            Console.WriteLine($"API response after creation a user: {response.Content}");
            if (response.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(response.Content) && JsonConvert.DeserializeObject<User>(response.Content) is User createdUser)
            {
                _context.UserId = createdUser.Id;
                _context.User = createdUser;
                _context.CreatedUserIds.Add(createdUser.Id);
            }
            else
            {
                Console.WriteLine($"User creation error: {response.StatusCode} {response.Content}");
            }
        }

        [When(@"I send a POST request to create the other user")]
        public void WhenISendPOSTRequestToCreateOtherUser()
        {
            var response = _usersApiClient.CreateUser(_context.OtherUser);
            _context.Response = response;
            Console.WriteLine($"API response after creation another user: {response.Content}");
            if (response.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(response.Content) && JsonConvert.DeserializeObject<User>(response.Content) is User createdUser)
            {
                _context.OtherUserId = createdUser.Id;
                _context.OtherUser = createdUser;
                _context.CreatedUserIds.Add(createdUser.Id);
            }
            else
            {
                Console.WriteLine($"Another user creation error: {response.StatusCode} {response.Content}");
            }
        }

        [When(@"I send a PUT request to update the user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void WhenISendPutRequestToUpdateUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            var updatedUser = new User
            {
                Id = _context.UserId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };
            _context.Response = _usersApiClient.UpdateUser(_context.UserId, updatedUser);
        }

        [When(@"I send a PATCH request to update the user with email ""(.*)""")]
        public void WhenISendPatchRequestToUpdateUserWithEmail(string email)
        {
            var patchDto = new { Email = email };
            _context.Response = _usersApiClient.PatchUser(_context.UserId, patchDto);
        }

        [When(@"I send a DELETE request to delete the user")]
        public void WhenISendDeleteRequestToDeleteUser()
        {
            Console.WriteLine($"Deletion the user with id: {_context.UserId}");
            _context.Response = _usersApiClient.DeleteUser(_context.UserId);
        }

        [When(@"I send a DELETE request to delete the user by id (\d+)")]
        public void WhenISendDeleteRequestToDeleteUserById(int id)
        {
            _context.Response = _usersApiClient.DeleteUser(id);
        }

        [When(@"I send a GET request to get the user by id")]
        public void WhenISendGetRequestToGetUserById()
        {
            Console.WriteLine($"Get the user with id: {_context.UserId}");
            _context.Response = _usersApiClient.GetUser(_context.UserId);
        }

        [When(@"I send a GET request to get the user by id (\d+)")]
        public void WhenISendGetRequestToGetUserById(int id)
        {
            _context.Response = _usersApiClient.GetUser(id);
        }

        [Then(@"the response status should be (.*)")]
        public void ThenResponseStatusShouldBe(int statusCode)
        {
            ((int)_context.Response.StatusCode).Should().Be(statusCode);
        }

        [Then(@"the response should contain ""(.*)""")]
        public void ThenResponseShouldContain(string expectedText)
        {
            _context.Response.Content.Should().Contain(expectedText);
        }
    }
}