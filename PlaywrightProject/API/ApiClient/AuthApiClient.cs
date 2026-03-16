using RestSharp;

public class AuthApiClient
{
    private readonly RestClient _client;

    public AuthApiClient(string baseUrl)
    {
        _client = new RestClient(baseUrl);
    }

    public string GetToken(string username, string password)
    {
        var request = new RestRequest(ApiConfig.Login, Method.Post);
        request.AddJsonBody(new { username, password });
        var response = _client.Execute(request);
        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
        return (string)obj.token;
    }
}