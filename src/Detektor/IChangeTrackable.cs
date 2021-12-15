namespace Detektor;

public interface IChangeTrackable<T> where T : class
{
    IChangeTracker<T> StartChangeTracking();
}
