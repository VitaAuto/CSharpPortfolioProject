using Amazon.SQS;
using Amazon.SQS.Model;
using ApiControllerProject.Controllers;
using ApiControllerProject.Models;
using ApiControllerProject.Repositories;
using ApiControllerProject.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiAndUiProject.Tests.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class UsersControllerTests
    {
        private Mock<IUserRepository>? _repoMock;
        private Mock<IMessageSender>? _messageSenderMock;
        private string _defaultQueueUrl = "test-queue-url";
        private UsersController? _controller;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IUserRepository>();
            _messageSenderMock = new Mock<IMessageSender>();
            _controller = new UsersController(_repoMock.Object, _messageSenderMock.Object, _defaultQueueUrl);
        }

        // --- CREATE ---
        [TestCase(null, "Ivanov", "ivan@mail.com", true, "FirstName is required.")]
        [TestCase("Ivan", null, "ivan@mail.com", true, "LastName is required.")]
        [TestCase("Ivan", "Ivanov", null, true, "Email is required.")]
        [TestCase("Ivan", "Ivanov", "invalidemail", true, "Email is not valid.")]
        public async Task CreateUser_InvalidFields_ReturnsBadRequest(
            string firstName, string lastName, string email, bool isActive, string expectedMessage)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };

            var result = await _controller!.CreateUser(user);

            result.Should().BeOfType<BadRequestObjectResult>();
            if (result is BadRequestObjectResult badRequest)
                badRequest.Value.Should().Be(expectedMessage);
        }

        [Test]
        public async Task CreateUser_EmailAlreadyExists_ReturnsConflict()
        {
            var user = new User
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            _repoMock!.Setup(r => r.EmailExists(user.Email, null)).Returns(true);

            var result = await _controller!.CreateUser(user);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        // --- GET BY ID ---
        [TestCase(42, false)]
        [TestCase(1, true)]
        public void GetUser_VariousCases_ReturnsExpectedResult(int userId, bool found)
        {
            var user = new User { Id = userId, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock!.Setup(r => r.GetUser(userId)).Returns(found ? user : null);

            var result = _controller!.GetUser(userId);

            if (found)
            {
                result.Should().BeOfType<OkObjectResult>();
                if (result is OkObjectResult ok)
                    ok.Value.Should().BeEquivalentTo(user);

            }
            else
            {
                result.Should().BeOfType<NotFoundResult>();
            }
        }

        // --- GET ALL ---
        [Test]
        public void GetAllUsers_ReturnsList()
        {
            var users = new List<User>
            {
                new() { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true }
            };
            _repoMock!.Setup(r => r.GetAllUsers()).Returns(users);

            var result = _controller!.GetAllUsers();

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo(users);
        }

        // --- UPDATE ---
        [TestCase(null, "Ivanov", "ivan@mail.com", true, "FirstName is required.")]
        [TestCase("Ivan", null, "ivan@mail.com", true, "LastName is required.")]
        [TestCase("Ivan", "Ivanov", null, true, "Email is required.")]
        [TestCase("Ivan", "Ivanov", "invalidemail", true, "Email is not valid.")]
        public async Task UpdateUser_InvalidFields_ReturnsBadRequest(
            string firstName, string lastName, string email, bool isActive, string expectedMessage)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };

            var result = await _controller!.UpdateUser(1, user);

            result.Should().BeOfType<BadRequestObjectResult>();
            if (result is BadRequestObjectResult badRequest)
                badRequest.Value.Should().Be(expectedMessage);
        }

        [Test]
        public async Task UpdateUser_EmailAlreadyExists_ReturnsConflict()
        {
            var user = new User
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            _repoMock!.Setup(r => r.EmailExists(user.Email, 1)).Returns(true);

            var result = await _controller!.UpdateUser(1, user);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        [Test]
        public async Task UpdateUser_UserNotFound_ReturnsNotFound()
        {
            var user = new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock!.Setup(r => r.EmailExists(user.Email, 1)).Returns(false);
            _repoMock!.Setup(r => r.GetUser(1)).Returns((User?)null);

            var result = await _controller!.UpdateUser(1, user);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task UpdateUser_NoChanges_ReturnsNoContent()
        {
            var user = new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock!.Setup(r => r.EmailExists(user.Email, 1)).Returns(false);
            _repoMock!.Setup(r => r.GetUser(1)).Returns(user);

            var result = await _controller!.UpdateUser(1, user);

            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task UpdateUser_Valid_ReturnsOk_AndSendsMessage()
        {
            var oldUser = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var newUser = new User { FirstName = "Petr", LastName = "Petrov", Email = "petr@mail.com", IsActive = false };
            var updatedUser = new User { Id = 1, FirstName = "Petr", LastName = "Petrov", Email = "petr@mail.com", IsActive = false };

            _repoMock!.Setup(r => r.EmailExists(newUser.Email, 1)).Returns(false);
            _repoMock!.Setup(r => r.GetUser(1)).Returns(oldUser);
            _repoMock!.Setup(r => r.UpdateUser(1, newUser)).Returns(updatedUser);

            string? capturedQueueUrl = null;
            string? capturedMessageBody = null;
            IDictionary<string, string>? capturedAttributes = null;

            _messageSenderMock!.Setup(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Callback<string, string, IDictionary<string, string>>((queueUrl, messageBody, attributes) =>
                {
                    capturedQueueUrl = queueUrl;
                    capturedMessageBody = messageBody;
                    capturedAttributes = attributes;
                })
                .Returns(Task.CompletedTask);

            _controller!.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.UpdateUser(1, newUser);

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo(updatedUser);

            capturedQueueUrl.Should().Be(_defaultQueueUrl);
            capturedMessageBody.Should().NotBeNull();
            var sentUser = JsonSerializer.Deserialize<User>(capturedMessageBody!);
            sentUser.Should().BeEquivalentTo(updatedUser);
            capturedAttributes.Should().NotBeNull();
            capturedAttributes.Should().ContainKey("CorrelationId");
        }

        // --- PATCH ---
        [TestCase(null, null, null, null, "At least one field must be provided for patch.")]
        [TestCase("", null, null, null, "FirstName is required.")]
        [TestCase(null, "", null, null, "LastName is required.")]
        [TestCase(null, null, "", null, "Email is required.")]
        [TestCase(null, null, "invalidemail", null, "Email is not valid.")]
        public async Task PatchUser_InvalidFields_ReturnsBadRequest(
            string firstName, string lastName, string email, bool? isActive, string expectedMessage)
        {
            var patch = new UserPatchDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive

            };

            if (expectedMessage == "At least one field must be provided for patch.")
            {
                var result = await _controller!.PatchUser(1, patch);
                result.Should().BeOfType<BadRequestObjectResult>();
                if (result is BadRequestObjectResult badRequest)
                    badRequest.Value.Should().Be(expectedMessage);
                return;
            }

            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock!.Setup(r => r.GetUser(1)).Returns(user);

            var result2 = await _controller!.PatchUser(1, patch);
            result2.Should().BeOfType<BadRequestObjectResult>();
            if (result2 is BadRequestObjectResult badRequest2)
                badRequest2.Value.Should().Be(expectedMessage);
        }

        [Test]
        public async Task PatchUser_UserNotFound_ReturnsNotFound()
        {
            var patch = new UserPatchDto { FirstName = "NewName" };
            _repoMock!.Setup(r => r.GetUser(1)).Returns((User?)null);

            var result = await _controller!.PatchUser(1, patch);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task PatchUser_NoChanges_ReturnsNoContent()
        {
            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var patch = new UserPatchDto { FirstName = "Ivan" };
            _repoMock!.Setup(r => r.GetUser(1)).Returns(user);

            var result = await _controller!.PatchUser(1, patch);

            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task PatchUser_EmailAlreadyExists_ReturnsConflict()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            var patch = new UserPatchDto { Email = "petr@mail.com" };
            _repoMock!.Setup(r => r.GetUser(1)).Returns(user);
            _repoMock!.Setup(r => r.EmailExists("petr@mail.com", 1)).Returns(true);

            var result = await _controller!.PatchUser(1, patch);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        [Test]
        public async Task PatchUser_Valid_ReturnsOk_AndSendsMessage()
        {
            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var patch = new UserPatchDto { FirstName = "Petr" };
            var updatedUser = new User { Id = 1, FirstName = "Petr", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };

            _repoMock!.Setup(r => r.GetUser(1)).Returns(user);
            _repoMock!.Setup(r => r.EmailExists(It.IsAny<string>(), 1)).Returns(false);
            _repoMock!.Setup(r => r.PatchUser(1, It.IsAny<UserPatchDto>())).Returns(updatedUser);

            string? capturedQueueUrl = null;
            string? capturedMessageBody = null;
            IDictionary<string, string>? capturedAttributes = null;

            _messageSenderMock!.Setup(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Callback<string, string, IDictionary<string, string>>((queueUrl, messageBody, attributes) =>
                {
                    capturedQueueUrl = queueUrl;
                    capturedMessageBody = messageBody;
                    capturedAttributes = attributes;
                })
                .Returns(Task.CompletedTask);

            _controller!.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.PatchUser(1, patch);

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo(updatedUser);

            capturedQueueUrl.Should().Be(_defaultQueueUrl);
            capturedMessageBody.Should().NotBeNull();
            var sentUser = JsonSerializer.Deserialize<User>(capturedMessageBody!);
            sentUser.Should().BeEquivalentTo(updatedUser);
            capturedAttributes.Should().NotBeNull();
            capturedAttributes.Should().ContainKey("CorrelationId");
        }

        // --- DELETE ---
        [Test]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound_AndDoesNotSendMessage()
        {
            var userId = 1;
            _repoMock!.Setup(r => r.GetUser(userId)).Returns((User?)null);
            _repoMock!.Setup(r => r.DeleteUser(userId)).Returns(false);

            var result = await _controller!.DeleteUser(userId);

            result.Should().BeOfType<NotFoundResult>();
            _messageSenderMock!.Verify(s => s.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }
    }
}