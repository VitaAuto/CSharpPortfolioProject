using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace ApiPortfolioProject.Services
{
    public class VaultService : IVaultService
    {
        private readonly VaultClient _vaultClient;

        public VaultService(IConfiguration config)
        {
            var vaultUri = config["Vault:Uri"];
            var vaultToken = config["Vault:Token"];

            var authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);
            _vaultClient = new VaultClient(vaultClientSettings);
        }
        public async Task<(string Username, string Password)> GetCredentialsAsync()
        {
            var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myapp", mountPoint: "secret");
            var data = secret.Data.Data;

            var username = data.TryGetValue("username", out var usernameObj) ? usernameObj?.ToString() : null;
            var password = data.TryGetValue("password", out var passwordObj) ? passwordObj?.ToString() : null;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new Exception("Vault: 'username' or 'password' is missing or empty.");

            return (username, password);
        }

        public async Task<string> GetJwtSecretAsync()
        {
            var
         secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myapp", mountPoint: "secret");
            var data = secret.Data.Data;

            var jwtSecret = data.TryGetValue("jwtsecret", out var jwtSecretObj) ? jwtSecretObj?.ToString() : null;

            if (string.IsNullOrEmpty(jwtSecret))
                throw new Exception("Vault: 'jwtsecret' is missing or empty.");

            return jwtSecret;
        }
    }
}