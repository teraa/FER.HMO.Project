using System.Numerics;
using Solvers.Types;

// ReSharper disable InconsistentNaming

namespace Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class GetDiffData : TheoryData<Vector2, Vector2, int, float>
{
    public GetDiffData()
    {
        AddRow(Vector2.Zero, Vector2.Zero, 0, 0);
        AddRow(Vector2.Zero, Vector2.UnitX, 1, 1);
        AddRow(Vector2.UnitY, Vector2.UnitX, 2, Math.Sqrt(2));
        AddRow(Vector2.Zero, Vector2.One, 2, Math.Sqrt(2));
        AddRow(Vector2.Zero, -Vector2.One, 2, Math.Sqrt(2));
    }
}

public class CanAddData : TheoryData<Vector2, int, Vector2, int>
{
    public CanAddData()
    {
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        var customer = depot with {Id = 1};

        AddRow(depot, 0, customer, 0, "Zero");
        AddRow(depot, 1, customer with {Demand = 1}, 0, "Demand");
        AddRow(depot with {DueTime = 4}, 1, customer with {Position = Vector2.UnitX, Demand = 1, ReadyTime = 1, DueTime = 1, ServiceTime = 1}, 1, "Edge");
        AddRow(depot with {DueTime = 3}, 0, customer with {ServiceTime = 1}, 0, "Return edge");
        AddRow(depot with {DueTime = 2}, 0, customer with {Position = Vector2.UnitX, ReadyTime = 1, DueTime = 1}, 1, "Exact start");
    }

    private void AddRow(Customer depot, int capacity, Customer customer, int expectedStartTime, string message)
        => base.AddRow(message, depot, capacity, customer, expectedStartTime);
}

public class RouteTests
{
    private static readonly Customer _depot = new(0, Vector2.Zero, 0, 0, 0, 0);
    private static readonly Customer _customer = _depot with {Id = 1};

    [Fact]
    public void IsFinished_Empty()
    {
        var route = new Route(_depot, 0);
        route.IsFinished.Should().BeFalse();
    }

    [Fact]
    public void IsFinished_DepotToDepot()
    {
        var route = new Route(_depot, 0);
        route.Seal();
        route.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void IsFinishedByReference()
    {
        var route = new Route(_depot, 0);
        route.Add(route.Depot with { });
        route.IsFinished.Should().BeFalse();
    }

    [Fact]
    public void RemoveDepotByReference()
    {
        var route = new Route(_depot, 0);
        var depotClone = route.Depot with { };
        route.Add(depotClone);
        route.Seal();
        route.IsFinished.Should().BeTrue();
        route.Remove(depotClone);
        Assert.Throws<ArgumentException>(() => route.Remove(route.Depot));
    }

    [Theory]
    [ClassData(typeof(GetDiffData))]
    public void GetDiffTests(Vector2 start, Vector2 end, int expectedTime, float expectedDistance)
    {
        Route.GetDiff(start, end).Should().Be((expectedTime, expectedDistance));
    }

    [Theory]
    [ClassData(typeof(CanAddData))]
    public void CanAddTests(string message, Customer depot, int capacity, Customer customer, int expectedStartTime)
    {
        var route = new Route(depot, capacity);

        route.CanAdd(customer, out var stop).Should().BeTrue(message);
        stop!.ServiceStartedAt.Should().Be(expectedStartTime);
    }

    [Fact]
    public void Seal()
    {
        var depot = _depot;
        var route = new Route(depot, 0);
        route.Seal();
        route.Stops.Select(x => x.Customer).Should().Equal(depot, depot);
        Assert.Throws<InvalidOperationException>(() => route.Seal());
    }

    [Fact]
    public void CanAdd_OverCapacity()
    {
        var depot = _depot;
        var customer = _customer with {Demand = 1};
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var serviceStartTime).Should().BeFalse();
    }

    [Fact]
    public void CanAdd_OverDueTime()
    {
        var depot = _depot with {DueTime = 10};
        var customer = _customer with {Position = Vector2.UnitX};
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var serviceStartTime).Should().BeFalse();
    }

    [Fact]
    public void CanAdd_OverReturnDueTime()
    {
        var depot = _depot with {DueTime = 1};
        var customer = _customer with {ServiceTime = 2};
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var serviceStartTime).Should().BeFalse();
    }

    [Fact]
    public void TryInsert_IndexInRange()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Seal();

        route.TryInsert(customer, 1, out var newRoute).Should().BeTrue();
        newRoute!.Stops.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void TryInsert_IndexOutOfRange(int index)
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        Assert.Throws<IndexOutOfRangeException>(() => route.TryInsert(customer, index, out _));
    }

    [Fact]
    public void RemoveAt_IndexInRange()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        route.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);

        var newRoute = route.RemoveAt(1);
        newRoute.Stops.Select(x => x.Customer).Should().Equal(depot, depot);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void RemoveAt_IndexOutOfRange(int index)
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        route.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);

        Assert.Throws<IndexOutOfRangeException>(() => route.RemoveAt(index));
    }

    [Fact]
    public void Remove_Customer()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        route.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);

        var newRoute = route.Remove(customer);
        newRoute.Stops.Select(x => x.Customer).Should().Equal(depot, depot);
    }

    [Fact]
    public void Remove_Depot_Throws()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        route.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);

        Assert.Throws<ArgumentException>(() => route.Remove(depot));
    }

    [Fact]
    public void Remove_NonExisting_Throws()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Seal();

        route.Stops.Select(x => x.Customer).Should().Equal(depot, depot);

        Assert.Throws<ArgumentException>(() => route.Remove(customer));
    }

    [Fact]
    public void Split_Unfinished_Throws()
    {
        var route = new Route(_depot, 0);
        route.Add(_customer);

        Assert.Throws<InvalidOperationException>(() => route.Split());
    }

    [Fact]
    public void Split_Odd()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Seal();

        var parts = route.Split();
        parts.Item1.Stops.Select(x => x.Customer).Should().Equal(depot, depot);
        parts.Item2.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);
    }

    [Fact]
    public void Split_Even()
    {
        var depot = _depot;
        var customer1 = _customer;
        var customer2 = _customer with {Id = 2};
        var route = new Route(depot, 0);
        route.Add(customer1);
        route.Add(customer2);
        route.Seal();

        var parts = route.Split();
        parts.Item1.Stops.Select(x => x.Customer).Should().Equal(depot, customer1, depot);
        parts.Item2.Stops.Select(x => x.Customer).Should().Equal(depot, customer2, depot);
    }
}
