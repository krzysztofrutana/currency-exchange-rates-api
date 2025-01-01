using System.Reflection;
using Refit;

namespace CurrencyRates.Nbp.Helpers;

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