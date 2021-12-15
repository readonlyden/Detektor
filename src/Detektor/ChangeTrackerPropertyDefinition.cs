using System.Reflection;

namespace Detektor;

public class ChangeTrackerPropertyDefinition
{
    public PropertyInfo PropertyInfo { get; set; }
    public bool IsIgnored { get; set; }
    public Func<object?, string?> DisplayValueFunc { get; set; } = obj => obj?.ToString();

    public ChangeTrackerPropertyDefinition(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }
}
