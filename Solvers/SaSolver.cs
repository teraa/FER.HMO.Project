using System.Runtime.CompilerServices;
using Solvers.Types;

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

        int i = 0;
        double t = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;
            t *= 0.9;

            var customer = instance.Customers.Random(rnd);

            Solution? current;
            if (incumbent.Routes.Count >= previous.Routes.Count && rnd.NextDouble() < Math.Max(t, 0.001d))
                // if (false)
            {
                if (!previous.TryRemoveCustomer(customer, out current))
                    continue;
            }
            else
            {
                var route = previous.Routes.Random(rnd);
                var stopIndex = rnd.Next(1, route.Stops.Count - 1);

                if (!previous.TryMove(customer, route, stopIndex, out current))
                    continue;
            }

            if (current < incumbent)
            {
                yield return incumbent = current;
            }

            previous = current;
        }
    }
}
