using FluentAssertions;
using Newtonsoft.Json;
using Reqnroll;
using RestSharp;
using PlaywrightProject.API.Models;
using PlaywrightProject.API.TestData;
using PlaywrightProject.API.Context;
using System.Net;
using PlaywrightProject.API.ApiClient;
using PlaywrightProject.Config;

[Binding]
public class UsersApiSteps(UsersApiContext context)
{
    private readonly UsersApiContext _context = context;

    [Given(@"user is logged in")]
    public async Task GivenTheUserIsLoggedIn()
    {
        var vaultApiClient = new VaultApiClient(ApiConfig.VaultUri, ApiConfig.VaultToken);
        var (username, password) = await vaultApiClient.GetCredentialsAsync();

        var authApiClient = new AuthApiClient(ApiConfig.ApiBaseUrl);
        var token = authApiClient.GetToken(username, password);

        var usersApiClient = new UsersApiClient(ApiConfig.ApiBaseUrl);
        usersApiClient.SetToken(token);

        _context.ApiClient = usersApiClient;
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
        var resp = _context.ApiClient.CreateUser(_context.User);
        _context.Response = resp;
        Console.WriteLine($"API response after creation a user: {resp.Content}");
        if (resp.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(resp.Content) && JsonConvert.DeserializeObject<User>(resp.Content) is User createdUser)
        {
            _context.UserId = createdUser.Id;
            _context.User = createdUser;
            _context.CreatedUserIds.Add(createdUser.Id);
        }
        else
        {
            Console.WriteLine($"User creation error: {resp.StatusCode} {resp.Content}");
        }
    }

    [When(@"I send a POST request to create the other user")]
    public void WhenISendPOSTRequestToCreateOtherUser()
    {
        var resp = _context.ApiClient.CreateUser(_context.OtherUser);
        _context.Response = resp;
        Console.WriteLine($"API response after creation another user: {resp.Content}");
        if (resp.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(resp.Content) && JsonConvert.DeserializeObject<User>(resp.Content) is User createdUser)
        {
            _context.OtherUserId = createdUser.Id;
            _context.OtherUser = createdUser;
            _context.CreatedUserIds.Add(createdUser.Id);
        }
        else
        {
            Console.WriteLine($"Another user creation error: {resp.StatusCode} {resp.Content}");
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
        _context.Response = _context.ApiClient.UpdateUser(_context.UserId, updatedUser);
    }

    [When(@"I send a PATCH request to update the user with email ""(.*)""")]
    public void WhenISendPatchRequestToUpdateUserWithEmail(string email)
    {
        var patchDto = new { Email = email };
        _context.Response = _context.ApiClient.PatchUser(_context.UserId, patchDto);
    }

    [When(@"I send a DELETE request to delete the user")]
    public void WhenISendDeleteRequestToDeleteUser()
    {
        Console.WriteLine($"Deletion the user with id: {_context.UserId}");
        _context.Response = _context.ApiClient.DeleteUser(_context.UserId);
    }

    [When(@"I send a DELETE request to delete the user by id (\d+)")]
    public void WhenISendDeleteRequestToDeleteUserById(int id)
    {
        _context.Response = _context.ApiClient.DeleteUser(id);
    }

    [When(@"I send a GET request to get the user by id")]
    public void WhenISendGetRequestToGetUserById()
    {
        Console.WriteLine($"Get the user with id: {_context.UserId}");
        _context.Response = _context.ApiClient.GetUser(_context.UserId);
    }

    [When(@"I send a GET request to get the user by id (\d+)")]
    public void WhenISendGetRequestToGetUserById(int id)
    {
        _context.Response = _context.ApiClient.GetUser(id);
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