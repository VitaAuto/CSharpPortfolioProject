using dotenv.net;
using ApiAndUiProject.API.Clients;
using ApiAndUiProject.API.Context;
using Reqnroll;
using Serilog;

namespace ApiAndUiProject.Hooks
{
    [Binding]
    public class ApiHooks(ApiContext context, UsersApiClient usersApiClient, ILogger logger)
    {

        [AfterScenario]
        public void CleanupCreatedUsers()
        {
            var createdUserIds = context.Get<List<int>>("CreatedUserIds") ?? new List<int>();

            foreach (var id in createdUserIds.Distinct())
            {
                try
                {
                    var resp = usersApiClient.DeleteUser(id);
                    logger.Information("Deleted user with id: {UserId}, status: {StatusCode}", id, resp.StatusCode);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Failed to delete user with id: {id}");
                }
            }

            createdUserIds.Clear();
            context.Set("CreatedUserIds", createdUserIds);
        }
    }
}