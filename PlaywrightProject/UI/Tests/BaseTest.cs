using NUnit.Framework;

namespace PlaywrightProject.UI.Tests
{
    public abstract class BaseTest
    {
        protected PlaywrightDriver? Driver;

        [SetUp]
        public async Task SetUp()
        {
            Driver = new PlaywrightDriver();
            await Driver.InitAsync(1920, 1080);
        }

        [TearDown]
        public async Task TearDown()
        {
            if (Driver != null)
            {
                await Driver.CleanupAsync();
            }
        }
    }
}