using Newtonsoft.Json;
using PlaywrightProject.API.Auth;
using PlaywrightProject.API.Models;
using PlaywrightProject.Config;
using RestSharp;

namespace PlaywrightProject.API.ApiClient
{
    public class UsersApiClient(string baseUrl, ITokenProvider tokenProvider)
    {
        private readonly RestClient _client = new(baseUrl);
        private readonly ITokenProvider _tokenProvider = tokenProvider;

        private void AddAuthHeader(RestRequest request)
        {
            var token = _tokenProvider.GetToken();
            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");
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
}