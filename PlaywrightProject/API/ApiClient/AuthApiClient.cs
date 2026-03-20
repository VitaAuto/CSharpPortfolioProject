using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaywrightProject.Config;
using RestSharp;

namespace PlaywrightProject.API.ApiClient
{
    public class AuthApiClient(string baseUrl)
    {
        private readonly RestClient _client = new (baseUrl);

        public string GetToken(string? username, string? password)
        {
            var request = new RestRequest(ApiConfig.Login, Method.Post);
            request.AddJsonBody(new { username, password });
            var response = _client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                var jObj = JObject.Parse(response.Content);
                var token = jObj["token"]?.ToString();
                if (!string.IsNullOrEmpty(token))
                    return token;
                else
                    throw new InvalidOperationException("Token not found in response.");
            }
            else
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }
        }
    }
}