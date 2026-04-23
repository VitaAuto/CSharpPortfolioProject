using ApiAndUiProject.API.Clients;
using ApiAndUiProject.API.Models;
using Newtonsoft.Json;

namespace ApiAndUiProject.API.Services
{
    public class UserService(UsersApiClient usersApiClient)
    {
        public void EnsureUserEmailIsUnique(string email)
        {
            var response = usersApiClient.GetAllUsers();
            var content = response.Content ?? string.Empty;
            var users = JsonConvert.DeserializeObject<List<User>>(content) ?? [];
            var usersWithSameEmail = users.Where(u => u.Email == email).ToList();

            foreach (var user in usersWithSameEmail)
            {
                usersApiClient.DeleteUser(user.Id);
            }
        }
    }
}