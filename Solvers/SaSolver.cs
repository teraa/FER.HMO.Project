using System.Runtime.CompilerServices;

namespace Solvers;

public class SaSolver : ISolver
{
    public int? Seed { get; set; }
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
            var customer = instance.Customers[rnd.Next(0, instance.Customers.Count)];
            var route = previous.Routes[rnd.Next(0, previous.Routes.Count)];
            var index = rnd.Next(1, route.Stops.Count - 1);

            if (!previous.TryMove(customer, route, index, out var current))
                continue;

            if (current.Distance < incumbent.Distance || current.Routes.Count < incumbent.Routes.Count)
            {
                yield return incumbent = current;
            }

            previous = current;
        }
    }
}
