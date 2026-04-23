using FluentAssertions;
using NUnit.Framework;
using ApiAndUiProject.UI.Components;
using ApiAndUiProject.UI.Context;
using ApiAndUiProject.UI.Helpers;
using ApiAndUiProject.UI.Pages;
using Reqnroll;
using System.Threading.Tasks;

namespace ApiAndUiProject.Steps
{
    [Binding]
    public class UiSteps(PageContext context, IPageFactory pageFactory, IElementFinder elementFinder)
        {

        [Given(@"user opens '(.*)' page")]
        public async Task UserOpensPage(string pageName)
        {
            context.CurrentPage = pageFactory.CreatePageByName(pageName);
            await context.CurrentPage.OpenAsync();
            await context.CurrentPage.WaitForLoadAsync();
        }

        [Then(@"user should be navigated to '(.*)' page")]
        public async Task UserShouldBeNavigatedToPage(string pageName)
        {
            var expectedUrl = pageFactory.CreatePageByName(pageName).Url;
            var actualUrl = context.CurrentPage.GetCurrentUrl();
            await context.CurrentPage.WaitForLoadAsync();

            actualUrl.Should().StartWith(expectedUrl, $"Should be navigated to {pageName}");

            context.CurrentPage = pageFactory.CreatePageByName(pageName);
        }

        [When(@"user clicks ""(.*)""")]
        public async Task WhenUserClicksElement(string elementName)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should be present");

            await element.WaitForVisibleAsync(5000);
            (await element.IsVisibleAsync()).Should().BeTrue($"Element '{elementName}' should be visible before click");
            await element.ClickAsync();
        }

        [Then(@"""(.*)"" contains (.*) options")]
        public async Task ElementContainsOptions(string elementName, int optionsCount)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should be present");

            var actualCount = await element.GetOptionsCountAsync();
            actualCount.Should().Be(optionsCount, $"Element '{elementName}' should contain {optionsCount} options");
        }

        [Then(@"""(.*)"" should be present")]
        public async Task ThenElementShouldBePresent(string elementName)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should be present");

            await element.WaitForVisibleAsync(5000);
            (await element.IsVisibleAsync()).Should().BeTrue($"Element '{elementName}' should be present");
        }

        [Then(@"following options should be present:")]
        public async Task FollowingOptionsShouldBePresent(Table table)
        {
            foreach (var row in table.Rows)
            {
                var elementName = row[0];
                var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
                element.Should().NotBeNull($"Option '{elementName}' should be present");
                (await element.IsVisibleAsync()).Should().BeTrue($"Option '{elementName}' should be present");
            }
        }

        [When(@"user enters '(.*)' text in ""(.*)""")]
        public async Task UserEntersTextInElement(string text, string elementName)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            if (element is BaseTextField textField)
                await textField.FillAsync(text);
            else
                throw new Exception("Search Input is not a text field.");
        }

        [Then(@"search results (should|should not) be present in ""(.*)""")]
        public async Task SearchResultsInElementShouldBePresent(string should, string elementName)
        {
            var shouldBePresent = should == "should";
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull("Search results component should be present after searching");

            if (element is SearchResultsComponent resultsComponent)
            {
                if (shouldBePresent)
                {
                    await resultsComponent.WaitForResultsAsync();
                    var totalResults = await resultsComponent.GetTotalResultsCountAsync();
                    totalResults.Should().BeGreaterThan(0, "Search results should be present after searching");
                }
                else
                {
                    var message = await resultsComponent.WaitForNoResultsMessageAsync();
                    message.Should().Contain("Sorry, but your search returned no results. Please try another query.");
                }
            }
        }

        [When(@"user hovers over ""(.*)""")]
        public async Task WhenUserHoversOverElement(string elementName)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should be present");
            await element.HoverAsync();
        }

        [Then(@"hand pointer (should|should not) appear over ""(.*)""")]
        public async Task HandPointerAppearsOverElement(string should, string elementName)
        {
            var shouldAppear = should == "should";

            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should be present");
            await element.HoverAsync();
            var pointer = await element.GetCursorAsync();

            if (shouldAppear)
                pointer.Should().Be("pointer", $"Hand pointer should appear over '{elementName}' after hover");
            else
                pointer.Should().NotBe("pointer", $"Hand pointer should NOT appear over '{elementName}' after hover");
        }

        [Then(@"""(.*)"" should be hidden")]
        public async Task ElementShouldBeHidden(string elementName)
        {
            var element = elementFinder.FindElementByName(context.CurrentPage, elementName);
            element.Should().NotBeNull($"Element '{elementName}' should not be visible"); 
            await element.WaitForHiddenAsync();
            (await element.IsVisibleAsync()).Should().BeFalse($"Element '{elementName}' should not be visible");
        }
    }
}