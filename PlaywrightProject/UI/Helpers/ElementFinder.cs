using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PlaywrightProject.UI.Attributes;
using PlaywrightProject.UI.Components;

namespace PlaywrightProject.UI.Helpers
{
    public class ElementFinder : IElementFinder
    {
        public object? FindElementByName(object pageObject, string name)
        {
            var type = pageObject.GetType();

            var property = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(NameAttribute), false)
                    .Cast<NameAttribute>().Any(a => a.Value == name)
                    && p.GetIndexParameters().Length == 0);

            if (property != null)
                return property.GetValue(pageObject);

            var field = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(f => f.GetCustomAttributes(typeof(NameAttribute), false)
                    .Cast<NameAttribute>().Any(a => a.Value == name));

            if (field != null)
                return field.GetValue(pageObject);

            var method = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.GetCustomAttributes(typeof(NameAttribute), false)
                    .Cast<NameAttribute>().Any(a => a.Value == name)
                    && m.GetParameters().Length == 0);

            if (method != null)
            {
                var result = method.Invoke(pageObject, null);
                if (result is Task task)
                {
                    task.GetAwaiter().GetResult();
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                return result;
            }

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = prop.GetValue(pageObject);
                if (value == null) continue;
                if (value is BaseComponent)
                {
                    try
                    {
                        var nestedResult = FindElementByName(value, name);
                        if (nestedResult != null)
                            return nestedResult;
                    }
                    catch
                    {
                    }
                }
            }

            foreach (var fld in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = fld.GetValue(pageObject);
                if (value == null) continue;
                if (value is BaseComponent)
                {
                    try
                    {
                        var nestedResult = FindElementByName(value, name);
                        if (nestedResult != null)
                            return nestedResult;
                    }
                    catch
                    {
                    }
                }
            }

            throw new Exception($"Element or component with name '{name}' not found.");
        }
    }
}