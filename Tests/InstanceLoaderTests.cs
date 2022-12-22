using Solvers.Types;

namespace Tests;

public class InstanceLoaderTests
{
    [Theory]
    [InlineData("i1.txt", 50, 200, 70, 70, 634, 200)]
    [InlineData("i2.txt", 100, 200, 100, 100, 765, 400)]
    [InlineData("i3.txt", 150, 700, 150, 150, 3815, 600)]
    [InlineData("i4.txt", 200, 200, 200, 200, 1676, 800)]
    [InlineData("i5.txt", 250, 1000, 250, 250, 7697, 1000)]
    public void LoadInstance(string fileName, int vehicles, int capacity, int x, int y, int dueDate, int customers)
    {
        var instance = InstanceLoader.LoadFromFile(Path.Join("../../../../instances", fileName));

        instance.Vehicles.Should().Be(vehicles);
        instance.Capacity.Should().Be(capacity);
        instance.Depot.Position.X.Should().Be(x);
        instance.Depot.Position.Y.Should().Be(y);
        instance.Depot.DueTime.Should().Be(dueDate);
        instance.Customers.Count.Should().Be(customers);
    }
}
