using Microsoft.Playwright;
using PlaywrightProject.UI.Context;
using PlaywrightProject.UI.Helpers;
using Reqnroll;
using Reqnroll.BoDi;

namespace PlaywrightProject.Hooks
{
    [Binding]
    public class UiHooks(IObjectContainer container, IBrowser browser, TestContext testContext)
    {
        private readonly IObjectContainer _container = container;
        private readonly IBrowser _browser = browser;
        private readonly TestContext _testContext = testContext;
        private IBrowserContext? _browserContext;
        private IPage? _page;

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _browserContext = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });
            _page = await _browserContext.NewPageAsync();

            _container.RegisterInstanceAs(_browserContext);
            _container.RegisterInstanceAs(_page);
            _container.RegisterFactoryAs<IPageFactory>(c => new PageFactory(_page));

            _testContext.Page = _page;
        }

        [AfterScenario]
        public async Task AfterScenario() => await _browserContext.CloseAsync();
    }
}