public interface IVaultService
{
    Task<(string Username, string Password)> GetCredentialsAsync();
    Task<string> GetJwtSecretAsync();
}