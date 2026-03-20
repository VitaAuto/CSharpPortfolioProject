using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightProject.API.Auth
{
    public class TokenProvider : ITokenProvider
    {
        private string? _token;
        public string? GetToken() => _token;
        public void SetToken(string token) => _token = token;
    }
}
