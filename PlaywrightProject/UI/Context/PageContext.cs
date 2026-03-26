using Microsoft.Playwright;
using PlaywrightProject.UI.Pages;
using Reqnroll;

namespace PlaywrightProject.UI.Context
{
    public class PageContext
    {
        private readonly ScenarioContext _scenarioContext;

        public PageContext(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        public BasePage CurrentPage
        {
            get => _scenarioContext.ContainsKey(typeof(BasePage).FullName)
                ? _scenarioContext.Get<BasePage>()
                : null!;
            set => _scenarioContext.Set(value);
        }
    }
}