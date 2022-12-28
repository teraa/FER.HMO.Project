using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Solvers.Types;

[DebuggerDisplay("{DebuggerDisplay}")]
public class Solution
{
    private readonly List<Route> _routes;

    public Solution(int vehicles)
    {
        _routes = new();
        Vehicles = vehicles;
    }

    private Solution(List<Route> routes, int vehicles)
    {
        _routes = routes;
        Vehicles = vehicles;
    }

    public int Vehicles { get; }
    public IReadOnlyList<Route> Routes => _routes;
    public float Distance => _routes.Sum(x => x.Distance);

    private string DebuggerDisplay
        => $"Vehicles: {Routes.Count}/{Vehicles}, Distance: {Distance}";


    public static bool operator <(Solution left, Solution right)
        => left.Routes.Count < right.Routes.Count ||
           left.Routes.Count == right.Routes.Count && left.Distance < right.Distance;

    public static bool operator >(Solution left, Solution right)
        => left.Routes.Count > right.Routes.Count ||
           left.Routes.Count == right.Routes.Count && left.Distance > right.Distance;

    public static bool operator <=(Solution left, Solution right)
        => !(left > right);

    public static bool operator >=(Solution left, Solution right)
        => !(left < right);


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(_routes.Count.ToString());

        int i = 0;
        foreach (var route in _routes)
            sb.AppendLine($"{++i}: {route}");

        sb.AppendLine(Distance.ToString("F"));

        return sb.ToString();
    }

    public Route CreateRoute(Customer depot, int capacity)
    {
        // if (_routes.Count == Vehicles)
        //     throw new InvalidOperationException("Creating a new route would exceed the vehicle limit.");

        var route = new Route(depot, capacity);
        _routes.Add(route);
        return route;
    }

    public bool TryRemoveCustomer(Customer customer, [NotNullWhen(true)] out Solution? solution)
    {
        var sourceRoute = _routes
            .Single(r => r.Stops
                .Select(s => s.Customer)
                .Contains(customer));

        if (sourceRoute.Stops.Count == 3)
        {
            solution = null;
            return false;
        }

        var newSourceRoute = sourceRoute.Remove(customer);
        var newTargetRoute = new Route(sourceRoute.Depot, sourceRoute.Capacity);
        newTargetRoute.Add(customer);
        newTargetRoute.Seal();

        var routes = new List<Route>(_routes.Count + 1);
        routes.AddRange(_routes.Where(x => x != sourceRoute));
        routes.Add(newSourceRoute);
        routes.Add(newTargetRoute);

        solution = new Solution(routes, Vehicles);
        return true;
    }

    public bool TryMove(Customer customer, Route targetRoute, int targetIndex, [NotNullWhen(true)] out Solution? solution)
    {
        if (!_routes.Contains(targetRoute))
            throw new ArgumentException("Target route is not a part of this solution.", nameof(targetRoute));

        solution = null;

        if (!targetRoute.TryInsert(customer, targetIndex, out var newTargetRoute))
            return false;

        var sourceRoute = _routes.Single(
            r => r.Stops
                .Select(s => s.Customer)
                .Contains(customer));

        if (sourceRoute == targetRoute)
            return false;

        var newSourceRoute = sourceRoute.Remove(customer);

        var routes = _routes.ToList();

        int index = routes.IndexOf(targetRoute);
        routes[index] = newTargetRoute;

        index = routes.IndexOf(sourceRoute);
        if (newSourceRoute.Stops.Count == 2)
            routes.RemoveAt(index);
        else
            routes[index] = newSourceRoute;

        solution = new Solution(routes.ToList(), Vehicles);
        return true;
    }

    public bool TryMove(Customer customer, Customer target, [NotNullWhen(true)] out Solution? solution)
    {
        var targetRoute = Routes.First(x => x.Stops.Any(x => x.Customer == target));
        int i;
        for (i = 0; i < targetRoute.Stops.Count; i++)
            if (targetRoute.Stops[i].Customer == target)
                break;

        return TryMove(customer, targetRoute, i, out solution);
    }

    public Solution SplitRoute(Route route)
    {
        if (!_routes.Contains(route))
            throw new ArgumentException("Target route is not a part of this solution.", nameof(route));

        var routes = new List<Route>(_routes.Count + 1);
        for (int i = 0; i < _routes.Count; i++)
            if (_routes[i] != route)
                routes.Add(_routes[i]);

        var split = route.Split();
        routes.Add(split.Item1);
        routes.Add(split.Item2);

        var solution = new Solution(routes, Vehicles);
        return solution;
    }
};
