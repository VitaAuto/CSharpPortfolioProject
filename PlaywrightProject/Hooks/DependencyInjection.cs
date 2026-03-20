using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Auth;
using PlaywrightProject.API.Services;
using PlaywrightProject.Config;
using Reqnroll;
using Reqnroll.BoDi;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class DependencyInjection
    {
        [BeforeTestRun]
        public static void RegisterDependencies(ObjectContainer container)
        {
            container.RegisterTypeAs<TokenProvider, ITokenProvider>();
            container.RegisterFactoryAs<UsersApiClient>(c =>
                new UsersApiClient(ApiConfig.ApiBaseUrl, c.Resolve<ITokenProvider>()));
            container.RegisterTypeAs<UserService, UserService>();
        }
    }
}