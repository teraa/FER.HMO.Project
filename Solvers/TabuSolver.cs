using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class TabuSolver : ISolver
{
    // TODO: Fitness as:
    // 1. Vehicle count
    // 2. Count of lower stop count routes increased?
    // Currently 2. is Distance
    private readonly Queue<Customer> _tabu = new();

    public int Tenure { get; set; } = 50;
    public int? Seed { get; set; } = null;
    public ISolver InitialSolver { get; set; } = new GreedySolver();

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        var rnd = Seed is null
            ? new Random()
            : new Random(Seed.Value);

        var initial = await InitialSolver.SolveFinalAsync(instance, stoppingToken);
        var incumbent = initial;
        var previous = initial;

        yield return incumbent;

        while (!stoppingToken.IsCancellationRequested)
        {
            var customer = instance.Customers
                .Except(_tabu)
                .Skip(rnd.Next(0, instance.Customers.Count - _tabu.Count))
                .First();

            var route = previous.Routes[rnd.Next(0, previous.Routes.Count)];
            var index = rnd.Next(1, route.Stops.Count - 1);

            if (!previous.TryMove(customer, route, index, out var current))
                continue;

            if (current.Distance < incumbent.Distance || current.Routes.Count < incumbent.Routes.Count)
            {
                yield return incumbent = current;
            }

            if (current.Distance < previous.Distance || current.Routes.Count < previous.Routes.Count)
            {
                _tabu.Enqueue(customer);

                if (_tabu.Count > Tenure)
                    _tabu.Dequeue();
            }

            previous = current;
        }
    }
}
