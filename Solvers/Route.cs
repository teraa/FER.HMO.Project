using System.Numerics;

namespace Solvers;

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
    public int Time => _stops.Last().ServiceCompletedAt;
    public int Demand => _stops.Sum(x => x.Customer.Demand);

    public float Distance => _stops
        .Skip(1)
        .Select(x => x.Customer.Position)
        .Aggregate(
            seed: (Distance: 0f, Previous: _stops.First().Customer.Position),
            func: (acc, current) => (acc.Distance + Vector2.Distance(acc.Previous, current), current),
            resultSelector: acc => acc.Distance);

    private static int GetTravelTime(Vector2 a, Vector2 b)
    {
        var distance = Vector2.Distance(a, b);
        var travelTime = (int) Math.Ceiling(distance);
        return travelTime;
    }

    public bool CanAdd(Customer customer, out int serviceStartTime)
    {
        serviceStartTime = 0;

        // Capacity
        if (Demand + customer.Demand > Capacity)
            return false;

        serviceStartTime = Time + GetTravelTime(Position, customer.Position);

        // Service on time
        if (serviceStartTime > customer.DueTime)
            return false;

        // Wait if early
        if (serviceStartTime < customer.ReadyTime)
            serviceStartTime = customer.ReadyTime;

        if (ReferenceEquals(customer, Depot))
            return true;

        // Can return to depot in time
        var depotReturnTime =
            serviceStartTime + customer.ServiceTime + GetTravelTime(customer.Position, Depot.Position);

        if (depotReturnTime > Depot.DueTime)
            return false;

        return true;
    }

    public bool TryAdd(Customer customer)
    {
        if (!CanAdd(customer, out var serviceStartTime))
            return false;

        _stops.Add(new Stop(customer, serviceStartTime));
        return true;
    }

    public void Add(Customer customer)
    {
        if (!TryAdd(customer))
            throw new InvalidOperationException();
    }

    public override string ToString()
        => string.Join("->", _stops.Select(x => $"{x.Customer.Id}({x.ServiceStartedAt})"));
}
