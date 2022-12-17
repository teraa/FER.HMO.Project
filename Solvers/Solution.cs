using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Solvers;

public record Solution(IReadOnlyList<Route> Routes);

public class Route
{
    private List<Customer> _customers;

    public Route(Customer depot, int capacity)
    {
        Depot = depot;
        Capacity = capacity;
        _customers = new List<Customer>
        {
            depot,
        };

        Time = depot.ReadyTime + depot.ServiceTime;
        Debug.Assert(Time is 0);
    }

    public Customer Depot { get; }
    public int Capacity { get; }
    public IReadOnlyList<Customer> Customers => _customers;
    public int Time { get; private set; }
    public Vector2 Position => _customers[^1].Position;
    public int Demand => _customers.Sum(x => x.Demand);
    public float Distance => _customers
        .Skip(1)
        .Select(x => x.Position)
        .Aggregate(
            seed: (Distance: 0f, Previous: _customers[0].Position),
            func: (acc, current) => (acc.Distance + Vector2.Distance(acc.Previous, current), current),
            resultSelector: acc => acc.Distance);

    public bool CanAdd(Customer customer, out int time)
    {
        time = 0;

        // Capacity
        if (Demand + customer.Demand > Capacity)
            return false;

        var distance = Vector2.Distance(Position, customer.Position);
        var travelTime = (int)Math.Ceiling(distance);
        time = Time + travelTime;

        // Time
        if (time > customer.DueTime)
            return false;

        // Wait if early
        if (time < customer.ReadyTime)
            time = customer.ReadyTime;

        // Service
        time += customer.ServiceTime;

        return true;
    }

    public void Add(Customer customer)
    {
        if (!CanAdd(customer, out var time))
            throw new InvalidOperationException();

        _customers.Add(customer);
        Time = time;
    }
}
