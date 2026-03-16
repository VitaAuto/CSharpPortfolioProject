using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

public class VaultApiClient
{
    private readonly IVaultClient _vaultClient;

    public VaultApiClient(string vaultUri, string vaultToken)
    {
        var authMethod = new TokenAuthMethodInfo(vaultToken);
        var vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);
        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<(string Username, string Password)> GetCredentialsAsync()
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myapp", mountPoint: "secret");
        var data = secret.Data.Data;
        return (data["username"].ToString(), data["password"].ToString());
    }
}