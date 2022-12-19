using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Solvers;

[DebuggerDisplay("{DebuggerDisplay}")]
public class Route
{
    private readonly List<Stop> _stops;

    public Route(Customer depot, int capacity)
    {
        Capacity = capacity;
        _stops = new List<Stop>
        {
            new(depot, 0, 0),
        };
    }

    private Route(List<Stop> stops, int capacity)
    {
        if (stops.Count < 1)
            throw new ArgumentException("Must include at least one stop (depot).", nameof(stops));

        _stops = stops;
        Capacity = capacity;
    }

    public int Capacity { get; }
    public bool IsFinished => _stops.First().Customer == _stops.Last().Customer && _stops.Count > 1;
    public Customer Depot => _stops.First().Customer;
    public IReadOnlyList<Stop> Stops => _stops;
    public Vector2 Position => _stops.Last().Customer.Position;
    public int Time => _stops.Last().ServiceCompletedAt;
    public int Demand => _stops.Sum(x => x.Customer.Demand);

    private string DebuggerDisplay
        => $"Demand: {Demand}/{Capacity}, Stops: {_stops.Count}, Time: {Time}, Distance: {Distance}";

    public float Distance => _stops.Last().Distance;

    public static (int Time, float Distance) GetDiff(Vector2 a, Vector2 b)
    {
        var distance = Vector2.Distance(a, b);
        var travelTime = (int) Math.Ceiling(distance);
        return (travelTime, distance);
    }

    public bool CanAdd(Customer customer, [NotNullWhen(true)] out Stop? stop)
    {
        stop = null;

        if (_stops.Skip(1).Select(x => x.Customer).Contains(customer))
            return false;

        // Capacity
        if (Demand + customer.Demand > Capacity)
            return false;

        var diff = GetDiff(Position, customer.Position);
        var serviceStartTime = Time + diff.Time;

        // Service on time
        if (serviceStartTime > customer.DueTime)
            return false;

        // Wait if early
        if (serviceStartTime < customer.ReadyTime)
            serviceStartTime = customer.ReadyTime;

        stop = new Stop(customer, serviceStartTime, Distance + diff.Distance);

        if (customer == Depot)
            return true;

        // Can return to depot in time
        var depotReturnTime =
            serviceStartTime + customer.ServiceTime + GetDiff(customer.Position, Depot.Position).Time;

        if (depotReturnTime <= Depot.DueTime)
            return true;

        stop = null;
        return false;
    }

    public void Seal()
    {
        Add(Depot);
    }

    public bool TryAdd(Customer customer)
    {
        if (!CanAdd(customer, out var stop))
            return false;

        _stops.Add(stop);
        return true;
    }

    public void Add(Customer customer)
    {
        if (!TryAdd(customer))
            throw new InvalidOperationException();
    }

    public bool TryInsert(Customer customer, int index, [NotNullWhen(true)] out Route? newRoute)
    {
        if (!IsFinished)
            throw new InvalidOperationException("Route is not finished.");

        if (index < 1 || index > _stops.Count - 1)
            throw new IndexOutOfRangeException("Cannot insert before start or after end depot.");

        newRoute = null;
        var route = new Route(_stops.Take(index).ToList(), Capacity);

        if (!route.TryAdd(customer))
            return false;

        foreach (var stop in _stops.Skip(index))
            if (!route.TryAdd(stop.Customer))
                return false;

        newRoute = route;
        return true;
    }

    public Route RemoveAt(int index)
    {
        if (!IsFinished)
            throw new InvalidOperationException("Route is not finished.");

        // exclude depot
        if (index < 1 || index >= _stops.Count - 1)
            throw new IndexOutOfRangeException();

        var route = new Route(_stops.Take(index).ToList(), Capacity);
        foreach (var stop in _stops.Skip(index + 1))
            route.Add(stop.Customer);

        return route;
    }

    public Route Remove(Customer customer)
    {
        if (!IsFinished)
            throw new InvalidOperationException("Route is not finished.");

        if (customer == Depot)
            throw new ArgumentException("Cannot remove depot.", nameof(customer));

        int index = _stops.FindIndex(x => x.Customer == customer);
        if (index == -1)
            throw new ArgumentException("Specified customer is not a part of this route.", nameof(customer));

        var route = new Route(_stops.Take(index).ToList(), Capacity);
        foreach (var stop in _stops.Skip(index + 1))
            route.Add(stop.Customer);

        return route;
    }

    public override string ToString()
        => string.Join("->", _stops.Select(x => $"{x.Customer.Id}({x.ServiceStartedAt})"));
}
