using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightProject.UI.Helpers
{
    public interface IElementFinder
    {
        object? FindElementByName(object pageObject, string name);
    }
}
