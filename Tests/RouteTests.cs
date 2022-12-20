using System.Numerics;
// ReSharper disable InconsistentNaming

namespace Tests;

public class RouteTests
{
    private static readonly Customer _depot = new(0, Vector2.Zero, 0, 0, 0, 0);
    private static readonly Customer _customer = _depot with {Id = 1};

    [Fact]
    public void CanAdd_Zero()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var time).Should().BeTrue();
        time.Should().Be(0);
    }

    [Fact]
    public void CanAdd_Demand()
    {
        var depot = _depot;
        var customer = _customer with {Demand = 1};
        var route = new Route(depot, 1);

        route.CanAdd(customer, out var time).Should().BeTrue();
        time.Should().Be(0);
    }

    [Fact]
    public void CanAdd_Edge()
    {
        var depot = _depot with {DueTime = 4};
        var customer = _customer with {Position = Vector2.UnitX, Demand = 1, ReadyTime = 1, DueTime = 1, ServiceTime = 1};
        var route = new Route(depot, 1);

        route.CanAdd(customer, out var time).Should().BeTrue();
        time.Should().Be(1);
    }

    [Fact]
    public void CanAdd_ReturnEdge()
    {
        var depot = _depot with {DueTime = 3};
        var customer = _customer with {ServiceTime = 1};
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var time).Should().BeTrue();
        time.Should().Be(0);
    }

    [Fact]
    public void CanAdd_StartEdge()
    {
        var depot = _depot with {DueTime = 2};
        var customer = _customer with {Position = Vector2.UnitX, ReadyTime = 1, DueTime = 1};
        var route = new Route(depot, 0);

        route.CanAdd(customer, out var time).Should().BeTrue();
        time.Should().Be(1);
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
        route.Add(depot);

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
        route.Add(depot);

        Assert.Throws<IndexOutOfRangeException>(() => route.TryInsert(customer, index, out _));
    }

    [Fact]
    public void RemoveAt_IndexInRange()
    {
        var depot = _depot;
        var customer = _customer;
        var route = new Route(depot, 0);
        route.Add(customer);
        route.Add(depot);

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
        route.Add(depot);

        route.Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);

        Assert.Throws<IndexOutOfRangeException>(() => route.RemoveAt(index));
    }
}
