using Microsoft.Playwright;
using ApiAndUiProject.UI.Helpers;
using ApiAndUiProject.UI.Pages;
using Reqnroll;
using Reqnroll.BoDi;

namespace ApiAndUiProject.Hooks
{
    [Binding]
    public class UiHooks(IBrowser browser, ScenarioContext scenarioContext, IObjectContainer container)
    {

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            var browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });
            var page = await browserContext.NewPageAsync();

            scenarioContext.Set(browserContext);
            scenarioContext.Set(page);

            container.RegisterInstanceAs<IPageFactory>(new PageFactory(page));
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            var browserContext = scenarioContext.Get<IBrowserContext>();
            await browserContext.CloseAsync();

            var browserContextKey = typeof(IBrowserContext).FullName;
            if (browserContextKey != null)
                scenarioContext.Remove(browserContextKey);

            var pageKey = typeof(IPage).FullName;
            if (pageKey != null)
                scenarioContext.Remove(pageKey);

            var basePageKey = typeof(BasePage).FullName;
            if (basePageKey != null && scenarioContext.ContainsKey(basePageKey))
                scenarioContext.Remove(basePageKey);
        }
    }
}