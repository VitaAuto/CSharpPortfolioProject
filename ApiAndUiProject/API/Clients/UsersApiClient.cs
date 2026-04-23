using ApiAndUiProject.API.Auth;
using ApiAndUiProject.API.Models;
using ApiAndUiProject.Config;
using RestSharp;

namespace ApiAndUiProject.API.Clients
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

        public RestResponse CreateUser(User user, string correlationId)
        {
            var request = new RestRequest(ApiConfig.Users, Method.Post);
            AddAuthHeader(request);
            request.AddHeader("X-Correlation-Id", correlationId);
            request.AddJsonBody(user);
            return _client.Execute(request);
        }

        public RestResponse GetUser(int id)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Get);
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
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Put);
            AddAuthHeader(request);
            request.AddJsonBody(user);
            return _client.Execute(request);
        }

        public RestResponse PatchUser(int id, object patchDto)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Patch);
            AddAuthHeader(request);
            request.AddJsonBody(patchDto);
            return _client.Execute(request);
        }

        public RestResponse DeleteUser(int id)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Delete);
            AddAuthHeader(request);
            return _client.Execute(request);
        }

    }
}