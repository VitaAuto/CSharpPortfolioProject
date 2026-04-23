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
using Serilog;

namespace ApiAndUiProject.Steps
{
    [Binding]
    public class UsersApiSteps(ApiContext context, UserService userService, UsersApiClient usersApiClient, ITokenProvider tokenProvider, SqsService sqsService, ILogger logger)
    {

        [Given(@"user is logged in")]
        public async Task GivenUserIsLoggedIn()
        {
            logger.Information("Logging in user"); 
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
            logger.Information("Ensuring email is unique: {Email}", email); 
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
            logger.Information("User data preparation: {firstName}, {lastName}, {email}, {isActive}", firstName, lastName, email, isActive);
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
            logger.Information("User data preparation: {AnotherUser}", context.Get<User>("AnotherUser"));
        }

        [When(@"I send POST request to create user")]
        public void WhenISendPostRequestToCreateUser()
        {
            logger.Information("Creating user"); 
            var correlationId = Guid.NewGuid().ToString();
            context.Set("CorrelationId", correlationId);

            logger.Information("Creating user with CorrelationId: {CorrelationId}", correlationId);

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
                logger.Error("User creation failed: {StatusCode} {Content}", response.StatusCode, response.Content);
            }
        }

        [When(@"I send POST request to create another user")]
        public void WhenISendPostRequestToCreateAnoherUser()
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
                logger.Error("Another user creation failed: {StatusCode} {Content}", response.StatusCode, response.Content);
            }
        }

        [When(@"I send PUT request to update user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void WhenISendPutRequestToUpdateUserWithData(string firstName, string lastName, string email, bool isActive)
        {
            logger.Information("Updating user"); 
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
            logger.Information("Patching user email to: {Email}", email); 
            var patchDto = new { Email = email };
            context.Set("Response", usersApiClient.PatchUser(context.Get<int>("UserId"), patchDto));
        }

        [When(@"I send DELETE request to delete user")]
        public void WhenISendDeleteRequestToDeleteUser()
        {
            logger.Information("Deleting user"); 
            var userId = context.Get<int>("UserId");
            context.Set("Response", usersApiClient.DeleteUser(userId));
        }

        [When(@"I send DELETE request to delete user by id (\d+)")]
        public void WhenISendDeleteRequestToDeleteUserById(int id)
        {
            logger.Information("Deleting user by id: {UserId}", id); 
            context.Set("Response", usersApiClient.DeleteUser(id));
        }

        [When(@"I send GET request to get user by id")]
        public void WhenISendGetRequestToGetUserById()
        {
            logger.Information("Getting user by id"); 
            var userId = context.Get<int>("UserId");
            context.Set("Response", usersApiClient.GetUser(userId));
        }

        [When(@"I send GET request to get user by id (\d+)")]
        public void WhenISendGetRequestToGetUserById(int id)
        {
            logger.Information("Getting user by id: {UserId}", id); 
            context.Set("Response", usersApiClient.GetUser(id));
        }

        [Then(@"response status should be (.*)")]
        public void ThenResponseStatusShouldBe(int statusCode)
        {
            logger.Information("Checking response status: {StatusCode}", statusCode); 
            ((int)context.Get<RestResponse>("Response").StatusCode).Should().Be(statusCode);
        }

        [Then(@"response should contain ""(.*)""")]
        public void ThenResponseShouldContain(string expectedText)
        {
            logger.Information("Checking response contains: {ExpectedText}", expectedText); 
            context.Get<RestResponse>("Response").Content.Should().Contain(expectedText);
        }

        [Then(@"message with CorrelationId should be present in SQS")]
        public async Task ThenMessageWithCorrelationIdShouldBePresentInSqs()
        {
            logger.Information("Checking SQS for message with CorrelationId"); 
            var correlationId = context.Get<string>("CorrelationId");
            var message = await sqsService.GetMessageByCorrelationIdAsync(ApiConfig.SqsQueueUrl, correlationId);

            message.Should().NotBeNull($"Message with CorrelationId {correlationId} should be present in SQS");
            context.Set("SqsMessage", message);
            logger.Information("Message with CorrelationId {CorrelationId} has been retrieved from SQS", correlationId);
        }

        [Then(@"message with CorrelationId is cleared in SQS")]
        public async Task ThenMessageWithCorrelationIdIsClearedInSqs()
        {
            logger.Information("Clearing SQS message with CorrelationId"); 
            var message = context.Get<Message>("SqsMessage") ?? throw new InvalidOperationException("No SQS message found in context!");
            var receiptHandle = message.ReceiptHandle;

            await sqsService.DeleteMessageAsync(ApiConfig.SqsQueueUrl, receiptHandle);
            logger.Information("Message with CorrelationId {CorrelationId} has been deleted from SQS", context.Get<string>("CorrelationId"));
        }

        [Then(@"SQS message body should match user with first name ""(.*)"", last name ""(.*)"", email ""(.*)"", is active (.*)")]
        public void ThenSqsMessageBodyShouldMatchUserData(string firstName, string lastName, string email, bool isActive)
        {
            logger.Information("Checking SQS message body matches expected user"); 
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