using System.Numerics;

namespace Tests;

public class SolutionTests
{
    [Fact]
    public void CreateRoute_ExactLimit()
    {
        var solution = new Solution(1);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        solution.CreateRoute(depot, 0);
    }

    [Fact]
    public void CreateRoute_OverLimit()
    {
        var solution = new Solution(0);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        Assert.Throws<InvalidOperationException>(() => solution.CreateRoute(depot, 0));
    }

    [Fact]
    public void TryMove_InvalidTarget()
    {
        var solution = new Solution(0);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        var customer = depot with { };
        var route = new Route(depot, 0);

        Assert.Throws<ArgumentException>(() => solution.TryMove(customer, route, 0, out _));
    }

    [Fact]
    public void TryMove_TargetFull()
    {
        var solution = new Solution(2);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        var customer = depot with {Demand = 1};

        var route1 = solution.CreateRoute(depot, 1);
        route1.Add(customer);
        route1.Add(depot);

        var route2 = solution.CreateRoute(depot, 0);
        route2.Add(depot);

        solution.TryMove(customer, route2, 1, out _).Should().BeFalse();
    }

    [Fact]
    public void TryMove_KeepsNonEmpty()
    {
        var solution = new Solution(2);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        var customer1 = depot with {Id = 1};
        var customer2 = depot with {Id = 2};

        var route1 = solution.CreateRoute(depot, 0);
        route1.Add(customer1);
        route1.Add(customer2);
        route1.Add(depot);

        var route2 = solution.CreateRoute(depot, 0);
        route2.Add(depot);

        solution.TryMove(customer2, route2, 1, out var newSolution).Should().BeTrue();
        newSolution!.Routes.Should().HaveCount(2);
        newSolution.Routes[0].Stops.Select(x => x.Customer).Should().Equal(depot, customer1, depot);
        newSolution.Routes[1].Stops.Select(x => x.Customer).Should().Equal(depot, customer2, depot);
    }

    [Fact]
    public void TryMove_RemovesEmpty()
    {
        var solution = new Solution(2);
        var depot = new Customer(0, Vector2.Zero, 0, 0, 0, 0);
        var customer = depot with {Id = 1};

        var route1 = solution.CreateRoute(depot, 0);
        route1.Add(customer);
        route1.Add(depot);

        var route2 = solution.CreateRoute(depot, 0);
        route2.Add(depot);

        solution.TryMove(customer, route2, 1, out var newSolution).Should().BeTrue();
        newSolution!.Routes.Should().HaveCount(1);
        newSolution.Routes[0].Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);
    }
}
