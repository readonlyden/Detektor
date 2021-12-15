using System;
using System.Linq;
using Xunit;

namespace Detektor.Tests;

public class ChangeTrackerTests
{
    private class EmptyClass
    {
    }

    private class SimpleClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class AdvancedClass
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SimpleClass Inner { get; set; }
    }

    [Fact]
    public void GetChanges_ShouldReturnEmptyList_ForObjectWithoutProperties()
    {
        var obj = new EmptyClass();

        var changeTracker = new ChangeTrackerBuilder<EmptyClass>().Build(obj);

        var changes = changeTracker.GetChanges();

        Assert.Empty(changes);
    }

    [Fact]
    public void GetChanges_ShouldReturnEmptyList_WhenNoChangesAvailable()
    {
        var obj = new SimpleClass();

        var changeTracker = new ChangeTrackerBuilder<SimpleClass>().Build(obj);

        var changes = changeTracker.GetChanges();

        Assert.Empty(changes);
    }

    [Fact]
    public void GetChanges_ShouldReturnListOfBasicChanges()
    {
        var obj = new SimpleClass
        {
            Id = 0,
            Name = null
        };
        var changeTracker = new ChangeTrackerBuilder<SimpleClass>().Build(obj);

        obj.Id = 100500;
        obj.Name = "Foo";
        var changes = changeTracker.GetChanges();

        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Collection(changes,
            idChange =>
            {
                Assert.Equal("Id", idChange.Key);
                Assert.Equal("0", idChange.Value.OldValue);
                Assert.Equal("100500", idChange.Value.NewValue);
            },
            valueChange =>
            {
                Assert.Equal("Name", valueChange.Key);
                Assert.Null(valueChange.Value.OldValue);
                Assert.Equal("Foo", valueChange.Value.NewValue);
            });
    }

    [Fact]
    public void GetChanges_ShouldNotTrackIgnoredProperties()
    {
        var obj = new SimpleClass();
        var changeTracker = new ChangeTrackerBuilder<SimpleClass>()
            .Ignore(u => u.Name)
            .Build(obj);

        obj.Id = 100500;
        obj.Name = "Foo";
        var changes = changeTracker.GetChanges();

        Assert.NotEmpty(changes);
        Assert.Single(changes);
        Assert.Collection(changes,
            idChange =>
            {
                Assert.Equal("Id", idChange.Key);
                Assert.Equal("0", idChange.Value.OldValue);
                Assert.Equal("100500", idChange.Value.NewValue);
            });
    }

    [Fact]
    public void GetChanges_IgnoreAllShouldStopChangeTracking()
    {
        var obj = new SimpleClass();
        var changeTracker = new ChangeTrackerBuilder<SimpleClass>()
            .IgnoreAll()
            .Build(obj);

        obj.Id = 100500;
        obj.Name = "Foo";
        var changes = changeTracker.GetChanges();

        Assert.Empty(changes);
    }

    [Fact]
    public void GetChanges_Setup_ShouldReenableChangeTrackingForThatProperty()
    {
        var obj = new SimpleClass();
        var changeTracker = new ChangeTrackerBuilder<SimpleClass>()
            .IgnoreAll()
            .Setup(u => u.Id)
            .Build(obj);

        obj.Id = 100500;
        obj.Name = "Foo";
        var changes = changeTracker.GetChanges();

        Assert.NotEmpty(changes);
        Assert.Single(changes);
        Assert.Collection(changes,
            idChange =>
            {
                Assert.Equal("Id", idChange.Key);
                Assert.Equal("0", idChange.Value.OldValue);
                Assert.Equal("100500", idChange.Value.NewValue);
            });
    }

    [Fact]
    public void GetChanges_Setup_ShouldSupportCustomDisplayValueFunc()
    {
        var obj = new AdvancedClass()
        {
            Id = 5,
            Name = "Foo",
            Inner = new SimpleClass()
            {
                Id = 2,
                Name = "FooInner"
            }
        };

        var changeTracker = new ChangeTrackerBuilder<AdvancedClass>()
            .Ignore(p => p.Inner)
            .Ignore(p => p.Name)
            .Setup(p => p.Inner, inner => inner?.Id.ToString())
            .Build(obj);

        obj.Inner = new SimpleClass
        {
            Id = 8,
            Name = "Bla"
        };
        var changes = changeTracker.GetChanges();

        Assert.NotEmpty(changes);
        Assert.Single(changes);
        Assert.Collection(changes,
            innerChange =>
            {
                Assert.Equal("Inner", innerChange.Key);
                Assert.Equal("2", innerChange.Value.OldValue);
                Assert.Equal("8", innerChange.Value.NewValue);
            });
    }

    [Fact]
    public void ChangeTracker_ShouldNotAllow_DefinitionsFromOtherType()
    {
        var obj = new AdvancedClass();

        var invalidDefinitions = typeof(SimpleClass)
            .GetProperties()
            .ToDictionary(property => property.Name,
                property => new ChangeTrackerPropertyDefinition(property));

        Assert.Throws<ArgumentException>(() =>
        {
            var changeTracker = new ChangeTracker<AdvancedClass>(obj, invalidDefinitions);

            obj.Id = 2;

            var changes = changeTracker.GetChanges();
        });
    }
}
