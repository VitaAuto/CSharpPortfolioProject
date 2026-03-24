using FluentAssertions;
using PlaywrightProject.UI.Components;
using PlaywrightProject.UI.Context;
using PlaywrightProject.UI.Helpers;
using PlaywrightProject.UI.Pages;
using Reqnroll;
using System.Threading.Tasks;

namespace PlaywrightProject.Steps
{
    [Binding]
    public class CommonUiSteps(TestContext testContext, IPageFactory pageFactory, IElementFinder elementFinder)
    {
        private readonly TestContext _testContext = testContext;
        private readonly IPageFactory _pageFactory = pageFactory;
        private readonly IElementFinder _elementFinder = elementFinder;

        [Given(@"user opens '(.*)' page")]
        public async Task GivenUserIsOnPage(string pageName)
        {
            _testContext.CurrentPage = _pageFactory.CreatePageByName(pageName);
            await _testContext.CurrentPage.OpenAsync();
            await _testContext.CurrentPage.WaitForLoadAsync();
        }

        [Then(@"user should be navigated to '(.*)' page")]
        public Task ThenUserMovesToPage(string pageName)
        {
            var expectedUrl = _pageFactory.CreatePageByName(pageName).Url;
            var actualUrl = _testContext.CurrentPage.GetCurrentUrl();
            _testContext.CurrentPage.GetCurrentUrl().Should().Be(expectedUrl, $"Should be navigated to {pageName}");
            actualUrl.Should().Be(expectedUrl, $"Should be navigated to {pageName}");
            Console.WriteLine($"Current page is: {actualUrl}");
            return Task.CompletedTask;
        }

        [When(@"user clicks ""(.*)""")]
        public async Task WhenUserClicksElement(string elementName)
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, elementName);
            if (element is BaseButton button)
            {
                var count = await button.GetCountAsync();
                Console.WriteLine($"'{elementName}' count: {count}");

                var isVisible = await button.GetIsVisibleAsync();
                Console.WriteLine($"'{elementName}' is visible: {isVisible}");

                await button.WaitForVisibleAsync(5000);
                (await button.IsVisibleAsync()).Should().BeTrue($"Element '{elementName}' should be visible before click");
                await button.ClickAsync();
            }
            else
                throw new Exception($"Element '{elementName}' is not clickable.");
        }

        [Then(@"""(.*)"" should be present")]
        public async Task ThenElementShouldBePresent(string elementName)
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, elementName);
            if (element is BaseComponent component)
            {
                await component.WaitForVisibleAsync(5000);
                (await component.IsVisibleAsync()).Should().BeTrue($"Element '{elementName}' should be present");
            }
            else
            {
                element.Should().NotBeNull($"Element '{elementName}' should be present");
            }
        }

        [Then(@"following options should be present:")]
        public async Task ThenTheFollowingMenuOptionsShouldBePresent(Table table)
        {
            foreach (var row in table.Rows)
            {
                var elementName = row[0];
                var element = _elementFinder.FindElementByName(_testContext.CurrentPage, elementName);
                if (element is BaseComponent component)
                {
                    (await component.IsVisibleAsync()).Should().BeTrue($"Menu option '{elementName}' should be present");
                }
                else
                {
                    element.Should().NotBeNull($"Menu option '{elementName}' should be present");
                }
            }
        }

        [Then(@"user should be navigated to ""(.*)""")]
        public async Task ThenIShouldBeNavigatedTo(string expectedUrl)
        {
            await _testContext.CurrentPage.WaitForUrlAsync(expectedUrl);
            _testContext.CurrentPage.GetCurrentUrl().Should().Be(expectedUrl, $"Should be navigated to {expectedUrl}");
        }

        [When(@"user enters '(.*)' text")]
        public async Task WhenUserEntersText(string text)
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, "Search Input");
            if (element is BaseTextField textField)
                await textField.FillAsync(text);
            else
                throw new Exception("Search Input is not a text field.");
        }

        [Then(@"search results should be present")]
        public async Task ThenSearchResultsShouldBePresent()
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, "Search Results");
            if (element is SearchResultsComponent resultsComponent)
            {
                await resultsComponent.WaitForResultsAsync();
                var totalResults = await resultsComponent.GetTotalResultsCountAsync();
                Console.WriteLine($"Found: {totalResults} results");
                totalResults.Should().BeGreaterThan(0, "Search results should be present after searching");
            }
            else
            {
                element.Should().NotBeNull("Search results component should be present after searching");
            }
        }

        [Then(@"search results should not be present")]
        public async Task ThenSearchResultsShouldNotBePresent()
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, "Search Results");
            if (element is SearchResultsComponent resultsComponent)
            {
                var message = await resultsComponent.WaitForNoResultsMessageAsync();
                message.Should().Contain(
                    "Sorry, but your search returned no results. Please try another query."
                );
            }
            else
            {
                element.Should().NotBeNull("Search results component should be present after searching");
            }
        }

        [When(@"user hovers over ""(.*)""")]
        public async Task WhenUserHoversOverElement(string elementName)
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, elementName);
            if (element is BaseButton button)
                await button.HoverAsync();
            else
                throw new Exception($"Element '{elementName}' is not a button and cannot be hovered.");
        }

        [Then(@"hand pointer appears over ""(.*)""")]
        public async Task ThenHandPointerAppearsOverElement(string elementName)
        {
            var element = _elementFinder.FindElementByName(_testContext.CurrentPage, elementName);
            if (element is BaseButton button)
            {
                await button.HoverAsync();
                var cursor = await button.GetCursorAsync();
                cursor.Should().Be("pointer", $"Hand pointer (cursor: pointer) should appear over '{elementName}' after hover");
            }
            else
                throw new Exception($"Element '{elementName}' is not a button.");
        }

        [Then(@"""(.*)"" should be hidden")]
        public async Task PopupShouldNotBeVisible(string popupName)
        {
            var popup = _elementFinder.FindElementByName(_testContext.CurrentPage, popupName);
            if (popup is BaseComponent component)
            {
                await component.WaitForHiddenAsync();
                (await component.IsVisibleAsync()).Should().BeFalse($"Popup '{popupName}' should not be visible");
            }
            else
            {
                popup.Should().NotBeNull($"Popup '{popupName}' should not be visible");
            }
        }
    }
}