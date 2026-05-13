using ApiAndUiProject.API.Auth;
using ApiAndUiProject.API.Models;
using ApiAndUiProject.Config;
using RestSharp;

namespace ApiAndUiProject.API.Clients
{
    public class UsersApiClient
    {
        private readonly RestClient _client;
        private readonly ITokenProvider _tokenProvider;
        private string? _queueUrl;

        public UsersApiClient(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
            _client = new RestClient(ApiConfig.ApiBaseUrl);
        }

        public void SetQueueUrl(string queueUrl) => _queueUrl = queueUrl;

        private void AddAuthHeader(RestRequest request)
        {
            var token = _tokenProvider.GetToken();
            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");
        }

        private void AddQueueUrlHeader(RestRequest request)
        {
            if (!string.IsNullOrEmpty(_queueUrl))
                request.AddHeader("X-Queue-Url", _queueUrl);
        }

        public RestResponse CreateUser(User user, string correlationId)
        {
            var request = new RestRequest(ApiConfig.Users, Method.Post);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            request.AddHeader("X-Correlation-Id", correlationId);
            request.AddJsonBody(user);
            return _client.Execute(request);
        }

        public RestResponse GetUser(int id)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Get);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            return _client.Execute(request);
        }

        public RestResponse GetAllUsers()
        {
            var request = new RestRequest(ApiConfig.Users, Method.Get);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            return _client.Execute(request);
        }

        public RestResponse UpdateUser(int id, User user, string correlationId)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Put);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            request.AddHeader("X-Correlation-Id", correlationId);
            request.AddJsonBody(user);
            return _client.Execute(request);
        }

        public RestResponse PatchUser(int id, object patchDto)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Patch);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            request.AddJsonBody(patchDto);
            return _client.Execute(request);
        }

        public RestResponse DeleteUser(int id)
        {
            var request = new RestRequest(ApiConfig.UserByIdTemplate(id), Method.Delete);
            AddAuthHeader(request);
            AddQueueUrlHeader(request);
            return _client.Execute(request);
        }
    }
}