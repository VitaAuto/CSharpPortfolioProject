using Amazon.SQS.Model;
using ApiAndUiProject.API.Auth;
using ApiAndUiProject.API.Clients;
using ApiAndUiProject.API.Context;
using ApiAndUiProject.API.Models;
using ApiAndUiProject.API.Services;
using ApiAndUiProject.Config;
using FluentAssertions;
using Newtonsoft.Json;
using Reqnroll;
using RestSharp;
using System.Net;

namespace ApiAndUiProject.Steps
{
    [Binding]
    public class UsersApiSteps(ApiContext context, UserService userService, UsersApiClient usersApiClient, ITokenProvider tokenProvider, SqsService sqsService)
    {

        [Given(@"user is logged in")]
        public async Task GivenTheUserIsLoggedIn()
        {
            var vaultApiClient = new VaultApiClient(ApiConfig.VaultUri, ApiConfig.VaultToken);
            var (username, password) = await vaultApiClient.GetCredentialsAsync();

            var authApiClient = new AuthApiClient(ApiConfig.ApiBaseUrl);
            var token = authApiClient.GetToken(username, password);

            tokenProvider.SetToken(token);
        }

        [Given(@"user email ""(.*)"" is unique")]
        [Then(@"user email ""(.*)"" is unique")]
        public async Task UserEmailIsUnique(string email)
        {
            userService.EnsureUserEmailIsUnique(email);
            await sqsService.DeleteMessagesByEmailAsync(ApiConfig.SqsQueueUrl, email);
        }

        [Given(@"I have user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void GivenIHaveUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            context.Set("User", new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            });
        }

        [Given(@"I have another user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void GivenIHaveAnotherUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            context.Set("AnotherUser", new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            });
        }

        [When(@"I send POST request to create user")]
        public void WhenISendPOSTRequestToCreateUser()
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Set("CorrelationId", correlationId);

            var response = usersApiClient.CreateUser(context.Get<User>("User"), correlationId);
            context.Set("Response", response);

            if (response.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(response.Content) && JsonConvert.DeserializeObject<User>(response.Content) is User createdUser)
            {
                context.Set("UserId", createdUser.Id);
                context.Set("User", createdUser);

                var createdUserIds = context.Get<List<int>>("CreatedUserIds") ?? [];
                createdUserIds.Add(createdUser.Id);
                context.Set("CreatedUserIds", createdUserIds);
            }
            else
            {
                Console.WriteLine($"User creation failed: {response.StatusCode} {response.Content}");
            }
        }

        [When(@"I send POST request to create another user")]
        public void WhenISendPOSTRequestToCreateAnoherUser()
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Set("CorrelationId", correlationId);

            var response = usersApiClient.CreateUser(context.Get<User>("AnotherUser"), correlationId);
            context.Set("Response", response);

            if (response.StatusCode == HttpStatusCode.Created && !string.IsNullOrEmpty(response.Content) && JsonConvert.DeserializeObject<User>(response.Content) is User createdUser)
            {
                context.Set("AnotherUserId", createdUser.Id);
                context.Set("AnotherUser", createdUser);

                var createdUserIds = context.Get<List<int>>("CreatedUserIds") ?? [];
                createdUserIds.Add(createdUser.Id);
                context.Set("CreatedUserIds", createdUserIds);
            }
            else
            {
                Console.WriteLine($"Another user creation failed: {response.StatusCode} {response.Content}");
            }
        }

        [When(@"I send PUT request to update user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void WhenISendPutRequestToUpdateUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            var updatedUser = new User
            {
                Id = context.Get<int>("UserId"),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };
            context.Set("Response", usersApiClient.UpdateUser(context.Get<int>("UserId"), updatedUser));
        }

        [When(@"I send PATCH request to update user with email ""(.*)""")]
        public void WhenISendPatchRequestToUpdateUserWithEmail(string email)
        {
            var patchDto = new { Email = email };
            context.Set("Response", usersApiClient.PatchUser(context.Get<int>("UserId"), patchDto));
        }

        [When(@"I send DELETE request to delete user")]
        public void WhenISendDeleteRequestToDeleteUser()
        {
            var userId = context.Get<int>("UserId");
            context.Set("Response", usersApiClient.DeleteUser(userId));
        }

        [When(@"I send a DELETE request to delete the user by id (\d+)")]
        public void WhenISendDeleteRequestToDeleteUserById(int id)
        {
            context.Set("Response", usersApiClient.DeleteUser(id));
        }

        [When(@"I send GET request to get user by id")]
        public void WhenISendGetRequestToGetUserById()
        {
            var userId = context.Get<int>("UserId");
            context.Set("Response", usersApiClient.GetUser(userId));
        }

        [When(@"I send GET request to get user by id (\d+)")]
        public void WhenISendGetRequestToGetUserById(int id)
        {
            context.Set("Response", usersApiClient.GetUser(id));
        }

        [Then(@"response status should be (.*)")]
        public void ThenResponseStatusShouldBe(int statusCode)
        {
            ((int)context.Get<RestResponse>("Response").StatusCode).Should().Be(statusCode);
        }

        [Then(@"response should contain ""(.*)""")]
        public void ThenResponseShouldContain(string expectedText)
        {
            context.Get<RestResponse>("Response").Content.Should().Contain(expectedText);
        }

        [Then(@"message with CorrelationId should be present in SQS")]
        public async Task ThenMessageWithCorrelationIdShouldBePresentInSqs()
        {
            var correlationId = context.Get<string>("CorrelationId");
            var message = await sqsService.GetMessageByCorrelationIdAsync(ApiConfig.SqsQueueUrl, correlationId);

            message.Should().NotBeNull($"Message with CorrelationId {correlationId} should be present in SQS");
            context.Set("SqsMessage", message);
        }

        [Then(@"message with CorrelationId is cleared in SQS")]
        public async Task ThenMessageWithCorrelationIdIsClearedInSqs()
        {
            var message = context.Get<Message>("SqsMessage") ?? throw new InvalidOperationException("No SQS message found in context!");
            var receiptHandle = message.ReceiptHandle;

            await sqsService.DeleteMessageAsync(ApiConfig.SqsQueueUrl, receiptHandle);
        }

        [Then(@"SQS message body should match user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void ThenSqsMessageBodyShouldMatchUserData(string firstName, string lastName, string email, bool isActive)
        {
            var expectedUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };

            var message = context.Get<Message>("SqsMessage");
            var actualUser = JsonConvert.DeserializeObject<User>(message.Body);

            actualUser.Should().BeEquivalentTo(expectedUser, options => options
                .Excluding(u => u.Id)
                .Excluding(u => u.CreatedOn)
                .Excluding(u => u.ModifiedOn)
            );
        }
    }
}