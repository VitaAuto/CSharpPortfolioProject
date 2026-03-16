using RestSharp;
using PlaywrightProject.API.Models;

public class UsersApiClient
{
    private readonly RestClient _client;
    private string _token;

    public UsersApiClient(string baseUrl)
    {
        _client = new RestClient(baseUrl);
    }

    public void SetToken(string token)
    {
        _token = token;
    }

    private void AddAuthHeader(RestRequest request)
    {
        if (!string.IsNullOrEmpty(_token))
            request.AddHeader("Authorization", $"Bearer {_token}");
    }

    public RestResponse CreateUser(User user)
    {
        var request = new RestRequest(ApiConfig.Users, Method.Post);
        AddAuthHeader(request);
        request.AddJsonBody(user);
        return _client.Execute(request);
    }

    public RestResponse GetUser(int id)
    {
        var request = new RestRequest(ApiConfig.UserById.Replace("{id}", id.ToString()), Method.Get);
        AddAuthHeader(request);
        return _client.Execute(request);
    }

    public RestResponse GetAllUsers()
    {
        var request = new RestRequest(ApiConfig.Users, Method.Get);
        AddAuthHeader(request);
        return _client.Execute(request);
    }

    public RestResponse UpdateUser(int id, User user)
    {
        var request = new RestRequest(ApiConfig.UserById.Replace("{id}", id.ToString()), Method.Put);
        AddAuthHeader(request);
        request.AddJsonBody(user);
        return _client.Execute(request);
    }

    public RestResponse PatchUser(int id, object patchDto)
    {
        var request = new RestRequest(ApiConfig.UserById.Replace("{id}", id.ToString()), Method.Patch);
        AddAuthHeader(request);
        request.AddJsonBody(patchDto);
        return _client.Execute(request);
    }

    public RestResponse DeleteUser(int id)
    {
        var request = new RestRequest(ApiConfig.UserById.Replace("{id}", id.ToString()), Method.Delete);
        AddAuthHeader(request);
        return _client.Execute(request);
    }

}