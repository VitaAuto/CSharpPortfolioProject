using Microsoft.Playwright;
using PlaywrightProject.UI.Attributes;
using PlaywrightProject.UI.Components;

namespace PlaywrightProject.UI.Pages
{
    public class MainPage(IPage page) : BasePage(page)
    {
        public override string Url => "https://www.epam.com/";
        public HeaderComponent Header => new(Page);
        public FooterComponent Footer => new(Page);
        
        [Name("Cookie Consent popup")]
        public CookieConsentComponent CookieConsentPopup => new (Page);
    }
}