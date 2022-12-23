using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class SaSolver : ISolver
{
    public int? Seed { get; set; }
    public ISolver InitialSolver { get; set; } = new GreedySolver();
    public double InitialTemperature { get; set; } = 1000;

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

        double t = InitialTemperature;
        ulong i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;

            var customer = instance.Customers[rnd.Next(0, instance.Customers.Count)];
            var route = previous.Routes[rnd.Next(0, previous.Routes.Count)];
            var index = rnd.Next(1, route.Stops.Count - 1);

            if (!previous.TryMove(customer, route, index, out var current))
                continue;

            if (current.Routes.Count < incumbent.Routes.Count)
            {
                // Breakpoint
            }

            if (current.Distance < incumbent.Distance || current.Routes.Count < incumbent.Routes.Count)
            {
                yield return incumbent = current;
            }

            var d = Delta(previous, current);
            var p = Probability(t, d);

            if (rnd.NextDouble() < p)
            {
                previous = current;
            }

            Cool(ref t);
        }
    }

    private static double Delta(Solution previous, Solution current)
    {
        var d = (current.Routes.Count - previous.Routes.Count) * 1000
            + current.Distance - previous.Distance;

        return d;
    }

    private static void Cool(ref double t)
    {
        // t /= (1 + 0.2 * t);
        t *= 0.9;
    }

    private static double Probability(double t, double d)
    {
        if (d <= 0)
            return 1;

        return 0;

        // var exp = -d / t;
        // var p = Math.Exp(exp);
        // return p;
    }
}
