using dotenv.net;
using PlaywrightProject.API.Context;
using Reqnroll;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class ApiCleanupHooks(UsersApiContext context)
    {
        private readonly UsersApiContext _context = context;

        //[AfterScenario]
        //public void CleanupCreatedUsers()
        //{
        //    foreach (var id in _context.CreatedUserIds.Distinct())
        //    {
        //        try
        //        {
        //            var resp = _context.ApiClient.DeleteUser(id);
        //            Console.WriteLine($"[CLEANUP] Deleted user with id: {id}, status: {resp.StatusCode}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[CLEANUP] Failed to delete user with id: {id}. Error: {ex.Message}");
        //        }
        //    }
        //    _context.CreatedUserIds.Clear();
        //}
    }
}