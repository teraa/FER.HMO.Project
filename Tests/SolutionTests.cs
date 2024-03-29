using System.Numerics;
using Solvers.Types;

// ReSharper disable InconsistentNaming

namespace Tests;

public class SolutionTests
{
    [Fact]
    public void CreateRoute_ExactLimit()
    {
        var solution = new Solution(1);
        var depot = Data.Depot;
        solution.CreateRoute(depot, 0);
    }

    [Fact(Skip = "Disabled constraint")]
    public void CreateRoute_OverLimit()
    {
        var solution = new Solution(0);
        var depot = Data.Depot;
        Assert.Throws<InvalidOperationException>(() => solution.CreateRoute(depot, 0));
    }

    [Fact]
    public void TryMove_InvalidTarget()
    {
        var solution = new Solution(0);
        var depot = Data.Depot;
        var customer = depot with { };
        var route = new Route(depot, 0);

        Assert.Throws<ArgumentException>(() => solution.TryMove(customer, route, 0, out _));
    }

    [Fact]
    public void TryMove_TargetFull()
    {
        var solution = new Solution(2);
        var depot = Data.Depot;
        var customer = depot with {Demand = 1};

        var route1 = solution.CreateRoute(depot, 1);
        route1.Add(customer);
        route1.Seal();

        var route2 = solution.CreateRoute(depot, 0);
        route2.Seal();

        solution.TryMove(customer, route2, 1, out _).Should().BeFalse();
    }

    [Fact]
    public void TryMove_KeepsNonEmpty()
    {
        var solution = new Solution(2);
        var depot = Data.Depot;
        var customer1 = depot with {Id = 1};
        var customer2 = depot with {Id = 2};

        var route1 = solution.CreateRoute(depot, 0);
        route1.Add(customer1);
        route1.Add(customer2);
        route1.Seal();

        var route2 = solution.CreateRoute(depot, 0);
        route2.Seal();

        solution.TryMove(customer2, route2, 1, out var newSolution).Should().BeTrue();
        newSolution!.Routes.Should().HaveCount(2);
        newSolution.Routes[0].Stops.Select(x => x.Customer).Should().Equal(depot, customer1, depot);
        newSolution.Routes[1].Stops.Select(x => x.Customer).Should().Equal(depot, customer2, depot);
    }

    [Fact]
    public void TryMove_RemovesEmpty()
    {
        var solution = new Solution(2);
        var depot = Data.Depot;
        var customer = depot with {Id = 1};

        var route1 = solution.CreateRoute(depot, 0);
        route1.Add(customer);
        route1.Seal();

        var route2 = solution.CreateRoute(depot, 0);
        route2.Seal();

        solution.TryMove(customer, route2, 1, out var newSolution).Should().BeTrue();
        newSolution!.Routes.Should().HaveCount(1);
        newSolution.Routes[0].Stops.Select(x => x.Customer).Should().Equal(depot, customer, depot);
    }

    [Fact]
    public void TryMove_SameRoute()
    {
        var solution = new Solution(1);
        var depot = Data.Depot;
        var customer1 = depot with {Id = 1};
        var customer2 = depot with {Id = 2};

        var route = solution.CreateRoute(depot, 0);
        route.Add(customer1);
        route.Add(customer2);
        route.Seal();

        solution.TryMove(customer1, route, 2, out _).Should().BeFalse();
    }

    [Fact]
    public void Split_InvalidRoute()
    {
        var depot = Data.Depot;
        var solution = new Solution(0);
        var route = new Route(depot, 0);
        solution.CreateRoute(depot, 0);

        Assert.Throws<ArgumentException>(() => solution.SplitRoute(route));
    }

    [Fact]
    public void Split_ValidRoute()
    {
        var depot = Data.Depot;
        var solution = new Solution(0);
        solution.CreateRoute(depot, 0);

        solution.Routes.Should().HaveCount(1);

        var route = solution.Routes[0];
        route.Seal();
        var next = solution.SplitRoute(route);

        next.Routes.Should().HaveCount(2);
    }

    [Fact]
    public void TryRemoveCustomer_SingleCustomerInRoute()
    {
        var depot = Data.Depot;
        var customer = Data.Customer;
        var solution = new Solution(0);
        var route = solution.CreateRoute(depot, 0);
        route.Add(customer);
        route.Seal();

        solution.TryRemoveCustomer(customer, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRemoveCustomer_Success()
    {
        var depot = Data.Depot;
        var customer1 = Data.Customer with {Id = 1};
        var customer2 = Data.Customer with {Id = 2};
        var solution = new Solution(0);
        var route = solution.CreateRoute(depot, 0);
        route.Add(customer1);
        route.Add(customer2);
        route.Seal();

        solution.TryRemoveCustomer(customer2, out var newSolution).Should().BeTrue();
        newSolution!.Routes.Should().HaveCount(2);
        newSolution.Routes[0].Stops.Select(x => x.Customer).Should().Equal(depot, customer1, depot);
        newSolution.Routes[1].Stops.Select(x => x.Customer).Should().Equal(depot, customer2, depot);
    }

    [Fact]
    public void Operator_LessThan_SameRouteCount_LessDistance_True()
    {
        var depot = Data.Depot with {DueTime = 100};
        var customer = Data.Customer with {Position = Vector2.One, DueTime = 100};

        var solution1 = new Solution(0);
        var route1 = solution1.CreateRoute(depot, 0);
        route1.Seal();

        var solution2 = new Solution(0);
        var route2 = solution2.CreateRoute(depot, 0);
        route2.Add(customer);
        route2.Seal();

        (solution1 < solution2).Should().BeTrue();
    }

    [Fact]
    public void Operator_LessThan_SameRouteCount_SameDistance_False()
    {
        var depot = Data.Depot with {DueTime = 100};
        var customer = Data.Customer with {Position = Vector2.One, DueTime = 100};

        var solution1 = new Solution(0);
        var route1 = solution1.CreateRoute(depot, 0);
        route1.Add(customer);
        route1.Seal();

        var solution2 = new Solution(0);
        var route2 = solution2.CreateRoute(depot, 0);
        route2.Add(customer);
        route2.Seal();

        (solution1 < solution2).Should().BeFalse();
    }

    [Fact]
    public void Operator_LessThan_LessRouteCount_True()
    {
        var depot = Data.Depot with {DueTime = 100};
        var customer1 = Data.Customer with {Position = Vector2.One, DueTime = 100};
        var customer2 = customer1 with {Position = Vector2.One * 2};

        var solution1 = new Solution(0);
        var route1 = solution1.CreateRoute(depot, 0);
        route1.Add(customer1);
        route1.Add(customer2);
        route1.Seal();

        var solution2 = new Solution(0);
        solution2.CreateRoute(depot, 0).Seal();
        solution2.CreateRoute(depot, 0).Seal();

        (solution1 < solution2).Should().BeTrue();
    }

}
