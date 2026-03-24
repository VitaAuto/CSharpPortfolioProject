using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Models;
using Newtonsoft.Json;

namespace PlaywrightProject.API.Services
{
    public class UserService(UsersApiClient usersApiClient)
    {
        private readonly UsersApiClient _usersApiClient = usersApiClient;

        public void EnsureUserEmailIsUnique(string email)
        {
            var response = _usersApiClient.GetAllUsers();
            var users = JsonConvert.DeserializeObject<List<User>>(response.Content) ?? [];
            var usersWithSameEmail = users.Where(u => u.Email == email).ToList();

            foreach (var user in usersWithSameEmail)
            {
                _usersApiClient.DeleteUser(user.Id);
            }
        }
    }
}