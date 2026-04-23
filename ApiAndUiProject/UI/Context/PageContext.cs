using Microsoft.Playwright;
using ApiAndUiProject.UI.Pages;
using Reqnroll;

namespace ApiAndUiProject.UI.Context
{
    public class PageContext(ScenarioContext scenarioContext)
    {

        public BasePage CurrentPage
        {
            get
            {
                var basePageKey = typeof(BasePage).FullName;
                if (basePageKey != null && scenarioContext.ContainsKey(basePageKey))
                    return scenarioContext.Get<BasePage>();
                return null!;
            }
            set => scenarioContext.Set(value);
        }
    }
}