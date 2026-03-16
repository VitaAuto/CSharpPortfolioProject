using FluentAssertions;
using PlaywrightProject.UI.Pages;
using PlaywrightProject.UI.Components;
using PlaywrightProject.UI.Context;
using PlaywrightProject.UI.Helpers;
using Reqnroll;
using System.Threading.Tasks;

[Binding]
public class CommonUiSteps
{
    private readonly TestContext _testContext;

    public CommonUiSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"user opens '(.*)' page")]
    public async Task GivenUserIsOnPage(string pageName)
    {
        _testContext.CurrentPage = PageFactory.CreatePageByName(pageName, _testContext.Page);
        await _testContext.CurrentPage.OpenAsync();
        await _testContext.CurrentPage.WaitForLoadAsync();
    }

    [Then(@"user should be navigated to '(.*)' page")]
    public async Task ThenUserMovesToPage(string pageName)
    {
        string expectedUrl = pageName.StartsWith("http")
            ? pageName
            : pageName.ToLower() == "main"
                ? UiConfig.UiBaseUrl
                : $"{UiConfig.UiBaseUrl}{pageName.ToLower()}";

        _testContext.CurrentPage.GetCurrentUrl().Should().Be(expectedUrl, $"Should be navigated to {pageName}");
        Console.WriteLine($"Current page is: {expectedUrl}");
    }

    [When(@"user clicks ""(.*)""")]
    public async Task WhenUserClicksElement(string elementName)
    {
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, elementName);
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
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, elementName);
        if (element is BaseComponent component)
        {
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
            var element = ElementFinder.FindElementByName(_testContext.CurrentPage, elementName);
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
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, "Search Input");
        if (element is BaseTextField textField)
            await textField.FillAsync(text);
        else
            throw new Exception("Search Input is not a text field.");
    }

    [Then(@"search results should be present")]
    public async Task ThenSearchResultsShouldBePresent()
    {
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, "Search Results");
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
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, "Search Results");
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
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, elementName);
        if (element is BaseButton button)
            await button.HoverAsync();
        else
            throw new Exception($"Element '{elementName}' is not a button and cannot be hovered.");
    }

    [Then(@"hand pointer appears over ""(.*)""")]
    public async Task ThenHandPointerAppearsOverElement(string elementName)
    {
        var element = ElementFinder.FindElementByName(_testContext.CurrentPage, elementName);
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
        var popup = ElementFinder.FindElementByName(_testContext.CurrentPage, popupName);
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