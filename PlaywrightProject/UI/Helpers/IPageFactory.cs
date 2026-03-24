using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightProject.UI.Helpers
{
    using PlaywrightProject.UI.Pages;

    public interface IPageFactory
    {
        T Create<T>() where T : BasePage;
        BasePage CreatePageByName(string pageName);
    }
}