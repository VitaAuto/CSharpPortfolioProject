using static Dapper.SqlMapper;
using ApiAndUiProject.Config;

namespace ApiAndUiProject.Config
{
    public static class ApiConfig
    {
        private static readonly CommonSettings _settings = ConfigReader.LoadSettings();
        
        public static string VaultUri => Environment.GetEnvironmentVariable("VAULT_URI") ?? "http://localhost:8200";
        public static string VaultToken => Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? _settings.VaultPass;
        public static string ApiBaseUrl => Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5043";
        public static string SqsQueueUrl => "http://localhost:4566/000000000000/my-queue";
        public static string SqsUrl => "http://localhost:4566";


        public const string Login = "/api/Auth/login";

        public const string Users = "/api/users";

        public const string UserById = "/api/users/{id}";
        public static string UserByIdTemplate(int id) => $"/api/users/{id}";
    }
}