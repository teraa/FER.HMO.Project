﻿using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Solvers;

public record Solution(IReadOnlyList<Route> Routes);

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

    public bool CanAdd(Customer customer, out int time)
    {
        time = 0;

        // Capacity
        if (Demand + customer.Demand > Capacity)
            return false;

        var distance = Vector2.Distance(Position, customer.Position);
        var travelTime = (int) Math.Ceiling(distance);
        time = _stops.Last().ServiceCompletedAt + travelTime;

        // Time
        if (time > customer.DueTime)
            return false;

        // Wait if early
        if (time < customer.ReadyTime)
            time = customer.ReadyTime;

        return true;
    }

    public void Add(Customer customer)
    {
        if (!CanAdd(customer, out var time))
            throw new InvalidOperationException();

        _stops.Add(new Stop(customer, time));
    }
}

public record Stop(Customer Customer, int ServiceStartedAt)
{
    public int ServiceCompletedAt => ServiceStartedAt + Customer.ServiceTime;
}
