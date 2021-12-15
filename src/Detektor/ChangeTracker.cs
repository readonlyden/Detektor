namespace Detektor;

public class ChangeTracker<T> : IChangeTracker<T> where T : class
{
    private readonly T _objectToTrack;
    private readonly Dictionary<string, object?> _initialValues;
    private readonly Dictionary<string, ChangeTrackerPropertyDefinition> _definitions;

    public ChangeTracker(T objectToTrack, Dictionary<string,
        ChangeTrackerPropertyDefinition> definitions)
    {
        _objectToTrack = objectToTrack;
        _definitions = definitions;
        EnsureDefinitionsAreForSelectedType();

        _initialValues = GetPropertyValues();
    }

    private void EnsureDefinitionsAreForSelectedType()
    {
        var type = typeof(T);

        if (_definitions.Any(def => def.Value.PropertyInfo.DeclaringType != type))
        {
            throw new ArgumentException("Definitions is not for declared type");
        }
    }

    public IReadOnlyDictionary<string, Change> GetChanges()
    {
        var changes = new Dictionary<string, Change>();
        var currentValues = GetPropertyValues();

        foreach (var (key, definition) in _definitions.Where(entry => !entry.Value.IsIgnored))
        {
            var initialValue = _initialValues[key];
            var currentValue = currentValues[key];

            if (!Equals(initialValue, currentValue))
            {
                changes.Add(key, new Change(
                    definition.DisplayValueFunc(initialValue),
                    definition.DisplayValueFunc(currentValue))
                );
            }
        }

        return changes;
    }

    private Dictionary<string, object?> GetPropertyValues()
    {
        return _definitions.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.PropertyInfo.GetValue(_objectToTrack)
        );
    }
}
