using Microsoft.Playwright;
using PlaywrightProject.UI.Attributes;
using PlaywrightProject.UI.Components;

public class CookieConsentComponent : BaseComponent
{
    public CookieConsentComponent(IPage page) : base(page, page.Locator("[aria-label='Cookie banner']")) { }

    [Name("Accept All Button")]
    public BaseButton AcceptAllButton => new (Page, Page.Locator("button#onetrust-accept-btn-handler"));

    [Name("Cookie Settings Button")]
    public BaseButton CookieSettingsButton => new (Page, Page.Locator("button#onetrust-pc-btn-handler.cookie-setting-link"));

    public override async Task<bool> IsVisibleAsync() => await Locator.IsVisibleAsync();
}