using PlaywrightProject.UI.Pages;
using Microsoft.Playwright;

namespace PlaywrightProject.UI.Helpers
{
    public class PageFactory : IPageFactory
    {
        private readonly IPage _playwrightPage;

        public PageFactory(IPage playwrightPage)
        {
            _playwrightPage = playwrightPage;
        }

        public T Create<T>() where T : BasePage
        {
            return (T)Activator.CreateInstance(typeof(T), _playwrightPage)!;
        }

        public BasePage CreatePageByName(string pageName)
        {
            return pageName.ToLower() switch
            {
                "main" => Create<MainPage>(),
                "services" => Create<ServicesPage>(),
                "insights" => Create<InsightsPage>(),
                "about" => Create<AboutPage>(),
                "careers" => Create<CareersPage>(),
                _ => throw new ArgumentException($"Page '{pageName}' is not defined.")
            };
        }
    }
}