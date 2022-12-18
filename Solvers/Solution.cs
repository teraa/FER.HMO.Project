using System.Text;

namespace Solvers;

public class Solution
{
    private readonly List<Route> _routes = new();

    public Solution(int vehicles)
    {
        Vehicles = vehicles;
    }

    public int Vehicles { get; }
    public IReadOnlyList<Route> Routes => _routes;
    public float Distance => _routes.Sum(x => x.Distance);

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
        if (_routes.Count == Vehicles)
            throw new InvalidOperationException();

        var route = new Route(depot, capacity);
        _routes.Add(route);
        return route;
    }
};
