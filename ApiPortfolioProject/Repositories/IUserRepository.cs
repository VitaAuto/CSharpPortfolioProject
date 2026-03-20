using ApiPortfolioProject.Models;
using System.Collections.Generic;

namespace ApiPortfolioProject.Repositories
{
    public interface IUserRepository
    {
        User? GetUser(int id);
        List<User> GetAllUsers();
        User? CreateUser(User user);
        User? UpdateUser(int id, User user);
        User? PatchUser(int id, UserPatchDto patch);
        bool DeleteUser(int id);
        bool EmailExists(string email, int? excludeId = null);
    }
}