using dotenv.net;
using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Context;
using Reqnroll;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class ApiHooks(ApiContext context, UsersApiClient usersApiClient)
    {
        private readonly ApiContext _context = context;
        private readonly UsersApiClient _usersApiClient = usersApiClient;

        [AfterScenario]
        public void CleanupCreatedUsers()
        {
            foreach (var id in _context.CreatedUserIds.Distinct())
            {
                try
                {
                    var resp = _usersApiClient.DeleteUser(id);
                    Console.WriteLine($"[CLEANUP] Deleted user with id: {id}, status: {resp.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLEANUP] Failed to delete user with id: {id}. Error: {ex.Message}");
                }
            }
            _context.CreatedUserIds.Clear();
        }
    }
}