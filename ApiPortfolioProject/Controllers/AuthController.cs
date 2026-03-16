using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiPortfolioProject.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IVaultService _vaultService;

        public AuthController(IVaultService vaultService)
        {
            _vaultService = vaultService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var creds = await _vaultService.GetCredentialsAsync();
            if (login.Username == creds.Username && login.Password == creds.Password)
            {
                var jwtSecret = await _vaultService.GetJwtSecretAsync();
                var token = GenerateJwtToken(login.Username, jwtSecret);
                return Ok(new { token });
            }
            return Unauthorized();
        }
        private string GenerateJwtToken(string username, string jwtSecret)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}