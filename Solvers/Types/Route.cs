using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Solvers.Types;

[DebuggerDisplay("{DebuggerDisplay}")]
public class Route
{
    private readonly List<Stop> _stops;

    public Route(Customer depot, int capacity)
    {
        Capacity = capacity;
        _stops = new List<Stop>
        {
            new(depot, 0, 0, 0),
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
    public bool IsFinished => First.Customer == Last.Customer && _stops.Count > 1;
    public Customer Depot => First.Customer;
    public IReadOnlyList<Stop> Stops => _stops;
    public Vector2 Position => Last.Customer.Position;
    public int Time => Last.ServiceCompletedAt;
    public float Distance => Last.Distance;
    public int Demand => Last.Demand;
    private Stop First => _stops.First();
    private Stop Last => _stops.Last();

    private string DebuggerDisplay
        => $"Demand: {Demand}/{Capacity}, Stops: {_stops.Count}, Time: {Time}, Distance: {Distance}";


    public static (int Time, float Distance) GetDiff(Vector2 a, Vector2 b)
    {
        var distance = Vector2.Distance(a, b);
        var travelTime = (int) Math.Ceiling(distance);
        return (travelTime, distance);
    }

    public bool CanAdd(Customer customer, [NotNullWhen(true)] out Stop? stop)
    {
        stop = null;

        for (int i = 1; i < _stops.Count; i++)
            if (_stops[i].Customer == customer)
                return false;

        var newDemand = Demand + customer.Demand;
        // Capacity
        if (newDemand > Capacity)
            return false;

        var diff = GetDiff(Position, customer.Position);
        var serviceStartTime = Time + diff.Time;

        // Service on time
        if (serviceStartTime > customer.DueTime)
            return false;

        // Wait if early
        if (serviceStartTime < customer.ReadyTime)
            serviceStartTime = customer.ReadyTime;

        stop = new Stop(customer, serviceStartTime, Distance + diff.Distance, newDemand);

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

        for (int i = index; i < _stops.Count; i++)
            if (!route.TryAdd(_stops[i].Customer))
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
        for (int i = index + 1; i < _stops.Count; i++)
            route.Add(_stops[i].Customer);

        return route;
    }

    public Route Remove(Customer customer)
    {
        if (!IsFinished)
            throw new InvalidOperationException("Route is not finished.");

        if (customer == Depot)
            throw new ArgumentException("Cannot remove depot.", nameof(customer));

        var stops = new List<Stop>();
        int i;
        for (i = 0; i < _stops.Count && _stops[i].Customer != customer; i++)
            stops.Add(_stops[i]);

        if (stops.Count == _stops.Count)
            throw new ArgumentException("Specified customer is not a part of this route.", nameof(customer));

        var route = new Route(stops, Capacity);
        for (i += 1; i < _stops.Count; i++)
            route.Add(_stops[i].Customer);

        return route;
    }

    public override string ToString()
        => string.Join("->", _stops.Select(x => $"{x.Customer.Id}({x.ServiceStartedAt})"));
}
