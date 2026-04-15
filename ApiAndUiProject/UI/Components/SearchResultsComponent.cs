using Microsoft.Playwright;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAndUiProject.UI.Components
{
    public class SearchResultsComponent : BaseComponent
    {
        public SearchResultsComponent(IPage page) : base(page, page.Locator("div.search-results__items").First) { }

        public async Task WaitForResultsAsync()
        {
            await Locator.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        public async Task<string?> WaitForNoResultsMessageAsync()
        {
            var locator = Page.Locator("div.search-results__exception-message.search-results--empty-result[role='alert']");
            try
            {
                await locator.WaitForAsync(new() { State = WaitForSelectorState.Visible });
                return await locator.InnerTextAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IReadOnlyList<ILocator>> GetResultItemsAsync()
        {
            var items = Locator.Locator("article.search-results__item");
            return await items.AllAsync();
        }

        public async Task<int> GetTotalResultsCountAsync()
        {
            var counterLocator = Page.Locator("h2.search-results__counter");
            if (await counterLocator.IsVisibleAsync())
            {
                var counterText = await counterLocator.InnerTextAsync();
                var match = System.Text.RegularExpressions.Regex.Match(counterText, @"(\d+)");
                if (match.Success)
                    return int.Parse(match.Value);
            }
            return 0;
        }

        public async Task<int> GetLoadedResultsCountAsync()
        {
            var items = Locator.Locator("article.search-results__item");
            return await items.CountAsync();
        }

        public async Task<bool> HasResultsAsync()
        {
            return await GetLoadedResultsCountAsync() > 0;
        }
    }
}