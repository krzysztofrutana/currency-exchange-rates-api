using System.Reflection;
using Refit;

namespace CurrencyRates.Nbp.Helpers;

/// <summary>
/// Potrzebne do Refita by poprawnie formatował daty dla NBP
/// </summary>
public class CustomDateUrlParameterFormatter : IUrlParameterFormatter
{
    public string Format(object value, ICustomAttributeProvider attributeProvider, Type type)
    {
        if (value is DateOnly dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        return value?.ToString();
    }
}