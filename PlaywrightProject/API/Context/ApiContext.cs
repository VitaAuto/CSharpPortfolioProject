using Microsoft.Playwright;
using PlaywrightProject.API.Models;
using RestSharp;
using Reqnroll;

namespace PlaywrightProject.API.Context
{
    public class ApiContext
    {
        private readonly ScenarioContext _scenarioContext;

        public ApiContext(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        public User User
        {
            get => _scenarioContext.ContainsKey("User") ? _scenarioContext.Get<User>("User") : null!;
            set => _scenarioContext.Set(value, "User");
        }

        public User OtherUser
        {
            get => _scenarioContext.ContainsKey("OtherUser") ? _scenarioContext.Get<User>("OtherUser") : null!;
            set => _scenarioContext.Set(value, "OtherUser");
        }

        public int UserId
        {
            get => _scenarioContext.ContainsKey("UserId") ? _scenarioContext.Get<int>("UserId") : 0;
            set => _scenarioContext.Set(value, "UserId");
        }

        public int OtherUserId
        {
            get => _scenarioContext.ContainsKey("OtherUserId") ? _scenarioContext.Get<int>("OtherUserId") : 0;
            set => _scenarioContext.Set(value, "OtherUserId");
        }

        public List<int> CreatedUserIds
        {
            get => _scenarioContext.ContainsKey("CreatedUserIds") ? _scenarioContext.Get<List<int>>("CreatedUserIds") : new List<int>();
            set => _scenarioContext.Set(value, "CreatedUserIds");
        }

        public RestResponse Response
        {
            get => _scenarioContext.ContainsKey("Response") ? _scenarioContext.Get<RestResponse>("Response") : null!;
            set => _scenarioContext.Set(value, "Response");
        }
    }
}