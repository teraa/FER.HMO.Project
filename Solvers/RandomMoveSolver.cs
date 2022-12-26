using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class RandomMoveSolver : ISolver
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
            var customer = instance.Customers.Random(rnd);
            var route = previous.Routes.Random(rnd);
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
