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
