using Microsoft.Playwright;
using PlaywrightProject.UI.Helpers;
using PlaywrightProject.UI.Pages;
using Reqnroll;
using Reqnroll.BoDi;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class UiHooks
    {
        private readonly IBrowser _browser;
        private readonly ScenarioContext _scenarioContext;
        private readonly IObjectContainer _container;

        public UiHooks(IBrowser browser, ScenarioContext scenarioContext, IObjectContainer container)
        {
            _browser = browser;
            _scenarioContext = scenarioContext;
            _container = container;
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            var browserContext = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });
            var page = await browserContext.NewPageAsync();

            _scenarioContext.Set(browserContext);
            _scenarioContext.Set(page);

            _container.RegisterInstanceAs<IPageFactory>(new PageFactory(page));
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            var browserContext = _scenarioContext.Get<IBrowserContext>();
            await browserContext.CloseAsync();

            _scenarioContext.Remove(typeof(IBrowserContext).FullName);
            _scenarioContext.Remove(typeof(IPage).FullName);
            if (_scenarioContext.ContainsKey(typeof(BasePage).FullName))
                _scenarioContext.Remove(typeof(BasePage).FullName);
        }
    }
}