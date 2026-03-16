using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

public class VaultService : IVaultService
{
    private readonly IVaultClient _vaultClient;

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
        Secret<SecretData> secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myapp", mountPoint: "secret");
        var data = secret.Data.Data;
        return (data["username"].ToString(), data["password"].ToString());
    }
    public async Task<string> GetJwtSecretAsync()
    {
        Secret<SecretData> secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("myapp", mountPoint: "secret");
        var data = secret.Data.Data;
        return data["jwtsecret"].ToString();
    }
}