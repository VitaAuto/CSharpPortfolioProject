using PlaywrightProject.UI.Pages;
using Microsoft.Playwright;

namespace PlaywrightProject.UI.Helpers
{
    public static class PageFactory
    {
        public static BasePage CreatePageByName(string pageName, IPage playwrightPage)
        {
            return pageName.ToLower() switch
            {
                "main" => new MainPage(playwrightPage),
                "services" => new ServicesPage(playwrightPage),
                "insights" => new InsightsPage(playwrightPage),
                "about" => new AboutPage(playwrightPage),
                "careers" => new CareersPage(playwrightPage),
                _ => throw new ArgumentException($"Page '{pageName}' is not defined.")
            };
        }
    }
}