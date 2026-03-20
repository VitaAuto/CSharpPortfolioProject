using NUnit.Framework;
using Moq;
using ApiPortfolioProject.Controllers;
using ApiPortfolioProject.Repositories;
using ApiPortfolioProject.Models;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using System.Collections.Generic;

namespace PlaywrightProject.Tests.UnitTests
{
    [TestFixture]
    public class UsersControllerTests
    {
        private Mock<IUserRepository>? _repoMock;
        private UsersController? _controller;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IUserRepository>();
            _controller = new UsersController(_repoMock.Object);
        }

        // --- CREATE ---
        [TestCase(null, "Ivanov", "ivan@mail.com", true, "FirstName is required.")]
        [TestCase("Ivan", null, "ivan@mail.com", true, "LastName is required.")]
        [TestCase("Ivan", "Ivanov", null, true, "Email is required.")]
        [TestCase("Ivan", "Ivanov", "invalidemail", true, "Email is not valid.")]
        public void CreateUser_InvalidFields_ReturnsBadRequest(
            string firstName, string lastName, string email, bool isActive, string expectedMessage)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };

            var result = _controller?.CreateUser(user);

            result.Should().BeOfType<BadRequestObjectResult>();
            if (result is BadRequestObjectResult badRequest)
                badRequest.Value.Should().Be(expectedMessage);
        }

        [Test]
        public void CreateUser_EmailAlreadyExists_ReturnsConflict()
        {
            var user = new User
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            _repoMock?.Setup(r => r.EmailExists(user.Email, null)).Returns(true);

            var result = _controller?.CreateUser(user);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        [Test]
        public void CreateUser_ValidUser_ReturnsCreated()
        {
            var user = new User
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            var createdUser = new User
            {
                Id = 1,
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            _repoMock?.Setup(r => r.EmailExists(user.Email, null)).Returns(false);
            _repoMock?.Setup(r => r.CreateUser(user)).Returns(createdUser);

            var result = _controller?.CreateUser(user);

            result.Should().BeOfType<CreatedAtActionResult>();
            if (result is CreatedAtActionResult created)
                created.Value.Should().BeEquivalentTo(createdUser);
        }

        // --- GET BY ID ---
        [TestCase(42, false)]
        [TestCase(1, true)]
        public void GetUser_VariousCases_ReturnsExpectedResult(int userId, bool found)
        {
            var user = new User { Id = userId, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock?.Setup(r => r.GetUser(userId)).Returns(found ? user : null);

            var result = _controller?.GetUser(userId);

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
                new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true }
            };
            _repoMock?.Setup(r => r.GetAllUsers()).Returns(users);

            var result = _controller?.GetAllUsers();

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo
(users);
        }

        // --- UPDATE ---
        [TestCase(null, "Ivanov", "ivan@mail.com", true, "FirstName is required.")]
        [TestCase("Ivan", null, "ivan@mail.com", true, "LastName is required.")]
        [TestCase("Ivan", "Ivanov", null, true, "Email is required.")]
        [TestCase("Ivan", "Ivanov", "invalidemail", true, "Email is not valid.")]
        public void UpdateUser_InvalidFields_ReturnsBadRequest(
            string firstName, string lastName, string email, bool isActive, string expectedMessage)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = isActive
            };

            var result = _controller?.UpdateUser(1, user);

            result.Should().BeOfType<BadRequestObjectResult>();
            if (result is BadRequestObjectResult badRequest)
                badRequest.Value.Should().Be(expectedMessage);
        }

        [Test]
        public void UpdateUser_EmailAlreadyExists_ReturnsConflict()
        {
            var user = new User
            {
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@mail.com",
                IsActive = true
            };
            _repoMock?.Setup(r => r.EmailExists(user.Email, 1)).Returns(true);

            var result = _controller?.UpdateUser(1, user);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        [Test]
        public void UpdateUser_UserNotFound_ReturnsNotFound()
        {
            var user = new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock?.Setup(r => r.EmailExists(user.Email, 1)).Returns(false);
            _repoMock?.Setup(r => r.GetUser(1)).Returns((User?)null);

            var result = _controller?.UpdateUser(1, user);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void UpdateUser_NoChanges_ReturnsNoContent()
        {
            var user = new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock?.Setup(r => r.EmailExists(user.Email, 1)).Returns(false);
            _repoMock?.Setup(r => r.GetUser(1)).Returns(user);

            var result = _controller?.UpdateUser(1, user);

            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public void UpdateUser_Valid_ReturnsOk()
        {
            var oldUser = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var newUser = new User { FirstName = "Petr", LastName = "Petrov", Email = "petr@mail.com", IsActive = false };
            var updatedUser = new User { Id = 1, FirstName = "Petr", LastName = "Petrov", Email = "petr@mail.com", IsActive = false };

            _repoMock?.Setup(r => r.EmailExists(newUser.Email, 1)).Returns(false);
            _repoMock?.Setup(r => r.GetUser(1)).Returns(oldUser);
            _repoMock?.Setup(r => r.UpdateUser(1, newUser)).Returns(updatedUser);

            var result = _controller?.UpdateUser(1, newUser);

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo(updatedUser);
        }

        // --- PATCH ---
        [TestCase(null, null, null, null, "At least one field must be provided for patch.")]
        [TestCase("", null, null, null, "FirstName is required.")]
        [TestCase(null, "", null, null, "LastName is required.")]
        [TestCase(null, null, "", null, "Email is required.")]
        [TestCase(null, null, "invalidemail", null, "Email is not valid.")]
        public void PatchUser_InvalidFields_ReturnsBadRequest(
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
                var result = _controller?.PatchUser(1, patch);
                result.Should().BeOfType<BadRequestObjectResult>();
                if (result is BadRequestObjectResult badRequest)
                    badRequest.Value.Should().Be(expectedMessage);
                return;
            }

            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            _repoMock?.Setup(r => r.GetUser(1)).Returns(user);

            var result2 = _controller?.PatchUser(1, patch);
            result2.Should().BeOfType<BadRequestObjectResult>();
            if (result2 is BadRequestObjectResult badRequest2)
                badRequest2.Value.Should().Be(expectedMessage);
        }

        [Test]
        public void PatchUser_UserNotFound_ReturnsNotFound()
        {
            var patch = new UserPatchDto { FirstName = "NewName" };
            _repoMock?.Setup(r => r.GetUser(1)).Returns((User?)null);

            var result = _controller?.PatchUser(1, patch);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void PatchUser_NoChanges_ReturnsNoContent()
        {
            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var patch = new UserPatchDto { FirstName = "Ivan" };
            _repoMock?.Setup(r => r.GetUser(1)).Returns(user);

            var result = _controller?.PatchUser(1, patch);

            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public void PatchUser_EmailAlreadyExists_ReturnsConflict()
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
            _repoMock?.Setup(r => r.GetUser(1)).Returns(user);
            _repoMock?.Setup(r => r.EmailExists("petr@mail.com", 1)).Returns(true);

            var result = _controller?.PatchUser(1, patch);

            result.Should().BeOfType<ConflictObjectResult>();
            if (result is ConflictObjectResult conflict)
                conflict.Value.Should().Be("User with this email already exists.");
        }

        [Test]
        public void PatchUser_Valid_ReturnsOk()
        {
            var user = new User { Id = 1, FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };
            var patch = new UserPatchDto { FirstName = "Petr" };
            var updatedUser = new User { Id = 1, FirstName = "Petr", LastName = "Ivanov", Email = "ivan@mail.com", IsActive = true };

            _repoMock?.Setup(r => r.GetUser(1)).Returns(user);
            _repoMock?.Setup(r => r.EmailExists("ivan@mail.com", 1)).Returns(false);
            _repoMock?.Setup(r => r.PatchUser(1, patch)).Returns(updatedUser);

            var result = _controller?.PatchUser(1, patch);

            result.Should().BeOfType<OkObjectResult>();
            if (result is OkObjectResult ok)
                ok.Value.Should().BeEquivalentTo(updatedUser);
        }

        // --- DELETE ---
        [TestCase(1, false)]
        [TestCase(2, true)]
        public void DeleteUser_VariousCases_ReturnsExpectedResult(int userId, bool deleted)
        {
            _repoMock?.Setup(r => r.DeleteUser(userId)).Returns(deleted);

            var result = _controller?.DeleteUser(userId);

            if (deleted)
                result.Should().BeOfType<NoContentResult>();
            else
                result.Should().BeOfType<NotFoundResult>();
        }
    }
}