using Detektor;

var user = new User("Johnny", "Cage");

// Enable change tracking
var changeTracker = user.StartChangeTracking();

// Do some changes
user.FirstName = "Johnatan";
user.LastName = "Hero";

// Get changes
var changes = changeTracker.GetChanges();

foreach (var change in changes)
{
    Console.WriteLine($"Property: {change.Key}");
    Console.WriteLine($"OldValue: {change.Value.OldValue}");
    Console.WriteLine($"NewValue: {change.Value.NewValue}");
}

class User: IChangeTrackable<User>
{ 
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public User(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public IChangeTracker<User> StartChangeTracking() =>
        new ChangeTrackerBuilder<User>().Build(this);
}
