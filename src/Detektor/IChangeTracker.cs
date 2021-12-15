namespace Detektor;

public interface IChangeTracker<T> where T : class
{
    IReadOnlyDictionary<string, Change> GetChanges();
}
