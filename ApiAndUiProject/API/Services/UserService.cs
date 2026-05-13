using ApiAndUiProject.API.Clients;
using ApiAndUiProject.API.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ApiAndUiProject.API.Services
{
    public class UserService
    {
        public void EnsureUserEmailIsUnique(UsersApiClient usersApiClient, string email)
        {
            var response = usersApiClient.GetAllUsers();
            var content = response.Content ?? string.Empty;
            var users = JsonConvert.DeserializeObject<List<User>>(content) ?? new List<User>();
            var usersWithSameEmail = users.Where(u => u.Email == email).ToList();

            foreach (var user in usersWithSameEmail)
            {
                usersApiClient.DeleteUser(user.Id);
            }
        }
    }
}