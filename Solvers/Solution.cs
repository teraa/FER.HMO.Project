using System.Numerics;
using System.Text;

namespace Solvers;

public class Solution
{
    public List<Route> Routes { get; } = new();
    public float Distance => Routes.Sum(x => x.Distance);
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Routes.Count.ToString());

        int i = 0;
        foreach (var route in Routes)
            sb.AppendLine($"{++i}: {route}");

        sb.AppendLine(Distance.ToString("F"));

        return sb.ToString();
    }
};

public class Route
{
    private List<Stop> _stops;

    public Route(Customer depot, int capacity)
    {
        Depot = depot;
        Capacity = capacity;
        _stops = new List<Stop>
        {
            new(depot, 0),
        };
    }

    public Customer Depot { get; }
    public int Capacity { get; }
    public IReadOnlyList<Stop> Stops => _stops;
    public Vector2 Position => _stops.Last().Customer.Position;
    public int Demand => _stops.Sum(x => x.Customer.Demand);

    public float Distance => _stops
        .Skip(1)
        .Select(x => x.Customer.Position)
        .Aggregate(
            seed: (Distance: 0f, Previous: _stops.First().Customer.Position),
            func: (acc, current) => (acc.Distance + Vector2.Distance(acc.Previous, current), current),
            resultSelector: acc => acc.Distance);

    public bool CanAdd(Customer customer, out int serviceStartTime)
    {
        serviceStartTime = 0;

        // Capacity
        if (Demand + customer.Demand > Capacity)
            return false;

        var distance = Vector2.Distance(Position, customer.Position);
        var travelTime = (int) Math.Ceiling(distance);
        serviceStartTime = _stops.Last().ServiceCompletedAt + travelTime;

        // Time
        if (serviceStartTime > customer.DueTime)
            return false;

        // Wait if early
        if (serviceStartTime < customer.ReadyTime)
            serviceStartTime = customer.ReadyTime;

        return true;
    }

    public void Add(Customer customer)
    {
        if (!CanAdd(customer, out var serviceStartTime))
            throw new InvalidOperationException();

        _stops.Add(new Stop(customer, serviceStartTime));
    }

    public override string ToString()
        => string.Join("->", _stops.Select(x => $"{x.Customer.Id}({x.ServiceStartedAt})"));
}

public record Stop(Customer Customer, int ServiceStartedAt)
{
    public int ServiceCompletedAt => ServiceStartedAt + Customer.ServiceTime;
}
