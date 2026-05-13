using ApiControllerProject.Models;
using ApiControllerProject.Repositories;
using ApiControllerProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ApiControllerProject.Controllers
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private const string EmailExistsError = "User with this email already exists.";
        private const string EmailInvalidError = "Email is not valid.";
        private readonly IUserRepository _userRepository;
        private readonly IMessageSender _messageSender;
        private readonly string _defaultQueueUrl;

        public UsersController(
            IUserRepository userRepository,
            IMessageSender messageSender,
            string defaultQueueUrl)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
            _defaultQueueUrl = defaultQueueUrl;
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
        }

        private string GetQueueUrl()
        {
            var queueUrlFromHeader = Request.Headers["X-Queue-Url"].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(queueUrlFromHeader) ? queueUrlFromHeader : _defaultQueueUrl;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest("User data is required.");
            if (string.IsNullOrWhiteSpace(user.FirstName))
                return BadRequest("FirstName is required.");
            if (string.IsNullOrWhiteSpace(user.LastName))
                return BadRequest("LastName is required.");
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required.");
            if (!IsValidEmail(user.Email))
                return BadRequest(EmailInvalidError);
            if (_userRepository.EmailExists(user.Email))
                return Conflict(EmailExistsError);
            if (string.IsNullOrWhiteSpace(user.CreatedOn))
                user.CreatedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrWhiteSpace(user.ModifiedOn))
                user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var createdUser = _userRepository.CreateUser(user);
            if (createdUser == null)
                return StatusCode(500, "User creation failed.");

            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            var messageBody = System.Text.Json.JsonSerializer.Serialize(createdUser);
            var attributes = new Dictionary<string, string> { { "CorrelationId", correlationId } };

            await _messageSender.SendMessageAsync(GetQueueUrl(), messageBody, attributes);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userRepository.GetUser(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userRepository.GetAllUsers();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (user == null)
                return BadRequest("User data is required.");
            if (string.IsNullOrWhiteSpace(user.FirstName))
                return BadRequest("FirstName is required.");
            if (string.IsNullOrWhiteSpace(user.LastName))
                return BadRequest("LastName is required.");
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required.");
            if (!IsValidEmail(user.Email))
                return BadRequest(EmailInvalidError);

            if (_userRepository.EmailExists(user.Email, id))
                return Conflict(EmailExistsError);

            var currentUser = _userRepository.GetUser(id);
            if (currentUser == null)
                return NotFound();

            if (currentUser.FirstName == user.FirstName &&
                currentUser.LastName == user.LastName &&
                currentUser.Email == user.Email &&
                currentUser.IsActive == user.IsActive)
            {
                return NoContent();
            }

            user.CreatedOn = currentUser.CreatedOn;
            user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var updatedUser = _userRepository.UpdateUser(id, user);
            if (updatedUser == null)
                return StatusCode(500, "User update failed.");

            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            var messageBody = System.Text.Json.JsonSerializer.Serialize(updatedUser);
            var attributes = new Dictionary<string, string> { { "CorrelationId", correlationId } };

            await _messageSender.SendMessageAsync(GetQueueUrl(), messageBody, attributes);

            return Ok(updatedUser);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] UserPatchDto patch)
        {
            if (patch == null)
                return BadRequest("Patch data is required.");

            if (patch.FirstName == null && patch.LastName == null && patch.Email == null && patch.IsActive == null)
                return BadRequest("At least one field must be provided for patch.");

            var user = _userRepository.GetUser(id);
            if (user == null)
                return NotFound();

            bool changed = false;
            if (patch.FirstName != null && patch.FirstName != user.FirstName) changed = true;
            if (patch.LastName != null && patch.LastName != user.LastName) changed = true;
            if (patch.Email != null && patch.Email != user.Email) changed = true;
            if (patch.IsActive.HasValue && patch.IsActive.Value != user.IsActive) changed = true;

            if (!changed)
                return NoContent();

            if (patch.FirstName != null && string.IsNullOrWhiteSpace(patch.FirstName))
                return BadRequest("FirstName is required.");
            if (patch.LastName != null && string.IsNullOrWhiteSpace(patch.LastName))
                return BadRequest("LastName is required.");
            if (patch.Email != null && string.IsNullOrWhiteSpace(patch.Email))
                return BadRequest("Email is required.");
            if (patch.Email != null && !IsValidEmail(patch.Email))
                return BadRequest(EmailInvalidError);

            var emailToCheck = patch.Email ?? user.Email;
            if (string.IsNullOrWhiteSpace(emailToCheck))
                return BadRequest("Email is required.");

            if (_userRepository.EmailExists(emailToCheck, id))
                return Conflict(EmailExistsError);

            if (patch.FirstName != null) user.FirstName = patch.FirstName;
            if (patch.LastName != null) user.LastName = patch.LastName;
            if (patch.Email != null) user.Email = patch.Email;
            if (patch.IsActive.HasValue) user.IsActive = patch.IsActive.Value;
            user.ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var updatedUser = _userRepository.PatchUser(id, patch);
            if (updatedUser == null)
                return StatusCode(500, "User patch failed.");

            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString();

            var messageBody = System.Text.Json.JsonSerializer.Serialize(updatedUser);
            var attributes = new Dictionary<string, string> { { "CorrelationId", correlationId } };

            await _messageSender.SendMessageAsync(GetQueueUrl(), messageBody, attributes);

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = _userRepository.GetUser(id);
            var deleted = _userRepository.DeleteUser(id);
            if (!deleted)
                return NotFound();

            if (user != null)
            {
                var deletedUser = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    IsActive = false,
                    ModifiedOn = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var messageBody = System.Text.Json.JsonSerializer.Serialize(deletedUser);
                var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(correlationId))
                    correlationId = Guid.NewGuid().ToString();

                var attributes = new Dictionary<string, string> { { "CorrelationId", correlationId } };

                await _messageSender.SendMessageAsync(GetQueueUrl(), messageBody, attributes);
            }

            return NoContent();
        }
    }
}