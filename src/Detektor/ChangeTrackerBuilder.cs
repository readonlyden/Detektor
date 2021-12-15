using System.Linq.Expressions;
using System.Reflection;

namespace Detektor;

public class ChangeTrackerBuilder<T> where T : class
{
    private readonly Dictionary<string, ChangeTrackerPropertyDefinition> _definitions = new();

    public ChangeTrackerBuilder()
    {
        foreach (var property in typeof(T).GetProperties())
        {
            _definitions.Add(property.Name, new ChangeTrackerPropertyDefinition(property)
            {
                IsIgnored = false,
            });
        }
    }

    public ChangeTrackerBuilder<T> Ignore<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            _definitions[propertyInfo.Name].IsIgnored = true;
        }

        return this;
    }

    public ChangeTrackerBuilder<T> IgnoreAll()
    {
        foreach (var entry in _definitions)
        {
            entry.Value.IsIgnored = true;
        }

        return this;
    }

    public ChangeTrackerBuilder<T> Setup<TProperty>(
        Expression<Func<T, TProperty>> expression, Func<TProperty?, string?>? displayFunc = null)
    {
        if (expression.Body is MemberExpression { Member: PropertyInfo propertyInfo })
        {
            _definitions[propertyInfo.Name].IsIgnored = false;

            if (displayFunc is not null)
            {
                _definitions[propertyInfo.Name].DisplayValueFunc = obj =>
                {
                    var property = (TProperty?)obj;
                    return displayFunc.Invoke(property);
                };
            }
        }

        return this;
    }

    public IChangeTracker<T> Build(T objectToTrack)
    {
        return new ChangeTracker<T>(objectToTrack, _definitions);
    }
}
