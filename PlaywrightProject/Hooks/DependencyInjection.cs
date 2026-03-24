using Microsoft.Playwright;
using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Auth;
using PlaywrightProject.API.Context;
using PlaywrightProject.API.Services;
using PlaywrightProject.Config;
using PlaywrightProject.UI.Context;
using PlaywrightProject.UI.Helpers;
using Reqnroll;
using Reqnroll.BoDi;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class DependencyInjection
    {
        [BeforeTestRun]
        public static async Task RegisterDependencies(ObjectContainer container)
        {
            var settings = ConfigReader.LoadSettings();

            var playwright = await Playwright.CreateAsync();

            IBrowser browser = settings?.Browser?.ToLower() switch
            {
                "chrome" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = settings.Headless }),
                "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = settings.Headless }),
                _ => throw new ArgumentException($"Unknown browser type: {settings?.Browser}")
            };

            container.RegisterInstanceAs(playwright);
            container.RegisterInstanceAs(browser);

            container.RegisterTypeAs<TestContext, TestContext>();
            container.RegisterTypeAs<ElementFinder, IElementFinder>();

            container.RegisterTypeAs<TokenProvider, ITokenProvider>();
            container.RegisterFactoryAs<UsersApiClient>(c => new UsersApiClient(ApiConfig.ApiBaseUrl, c.Resolve<ITokenProvider>()));
            container.RegisterTypeAs<UserService, UserService>();
            container.RegisterTypeAs<UsersApiContext, UsersApiContext>();
        }
    }
}