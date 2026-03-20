using Microsoft.Playwright;
using System.Threading.Tasks;

namespace PlaywrightProject.UI.Components
{
    public class BaseTextField(IPage page, ILocator locator) : BaseComponent(page, locator)
    {

        public async Task FillAsync(string text) => await Locator.FillAsync(text);

        public async Task ClearAsync() => await Locator.FillAsync("");

        public async Task<string> GetValueAsync() => await Locator.InputValueAsync();

        public async Task<bool> IsFocusedAsync() => await Locator.EvaluateAsync<bool>("el => document.activeElement === el");

        public async Task<bool> IsEnabledAsync() => await Locator.IsEnabledAsync();

        public override async Task<bool> IsVisibleAsync() => await Locator.IsVisibleAsync();

        public async Task<string?> GetPlaceholderAsync() => await Locator.GetAttributeAsync("placeholder");
    }
}