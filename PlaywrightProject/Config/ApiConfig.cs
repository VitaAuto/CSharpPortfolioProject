using static Dapper.SqlMapper;
using PlaywrightProject.Config;

namespace PlaywrightProject.Config
{
    public static class ApiConfig
    {
        private static readonly CommonSettings _settings = ConfigReader.LoadSettings();
        
        public static string VaultUri => Environment.GetEnvironmentVariable("VAULT_URI") ?? "http://localhost:8200";
        public static string VaultToken => Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? _settings.VaultPass;
        public static string ApiBaseUrl => Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5043";

        public const string Login = "/api/Auth/login";

        public const string Users = "/api/users";

        public const string UserById = "/api/users/{id}";
    }
}