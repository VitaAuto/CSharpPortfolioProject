using Microsoft.Playwright;
using System.Threading.Tasks;

namespace PlaywrightProject.UI.Pages
{
    public abstract class BasePage(IPage page)
    {
        protected readonly IPage Page = page;

        public abstract string Url { get; }

        public async Task OpenAsync()
        {
            await Page.GotoAsync(Url);
        }

        public async Task<string> GetTitleAsync()
        {
            return await Page.TitleAsync();
        }

        public async Task WaitForLoadAsync()
        {
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }

        public async Task WaitForUrlAsync(string urlPattern)
        {
            await Page.WaitForURLAsync(urlPattern);
        }

        public string GetCurrentUrl()
        {
            return Page.Url;
        }
    }
}