using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class GraspSolver : ISolver
{
    public Func<Stop, double> ValueFunc { get; set; } = x => x.ServiceStartedAt;
    public int? Seed { get; set; }
    public int Iterations { get; set; } = 10;
    public int MaxRclSize { get; set; } = 10;

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        await Task.Yield();

        var rnd = Seed is null
            ? new Random()
            : new Random(Seed.Value);

        var incumbent = null as Solution;
        for (int i = MaxRclSize; i >= 0; i--)
        {
            for (int j = 0; j < Iterations; j++)
            {
                var current = Construct(instance, rnd, i);

                if (incumbent is null || current < incumbent)
                {
                    yield return current;
                    incumbent = current;
                }
            }
        }

        if (incumbent is null)
            yield break;

        foreach (var solution in Improve(incumbent, instance, rnd, stoppingToken))
        {
            yield return solution;
        }
    }

    private static IEnumerable<Solution> Improve(Solution incumbent, Instance instance, Random rnd, CancellationToken stoppingToken)
    {
        var previous = incumbent;

        ulong i = 0;
        ulong j = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;

            var customer = instance.Customers.Random(rnd);
            var route = previous.Routes.Random(rnd);
            var index = rnd.Next(1, route.Stops.Count - 1);

            if (!previous.TryMove(customer, route, index, out var current))
                continue;

            if (current < incumbent)
            {
                j = i;
                yield return incumbent = current;
            }

            previous = current;
        }
    }

    private Solution Construct(Instance instance, Random rnd, int rclSize)
    {
        var solution = new Solution(instance.Vehicles);
        var unassigned = instance.Customers.ToList();
        var route = null as Route;

        while (unassigned.Any())
        {
            route ??= solution.CreateRoute(instance.Depot, instance.Capacity);

            // ReSharper disable once AccessToModifiedClosure
            var eligible = unassigned
                .Select(x => (
                    // ReSharper disable once VariableHidesOuterVariable
                    IsEligible: route.CanAdd(x, out var stop),
                    Stop: stop
                ))
                .Where(x => x.IsEligible)
                .Select(x => x.Stop!)
                .OrderBy(ValueFunc)
                .ToArray();

            if (eligible.Length == 0)
            {
                // return to depot and start a new route
                route.Seal();
                route = null;
            }
            else
            {
                var stop = eligible[rnd.Next(0, Math.Min(rclSize, eligible.Length))];

                route.Add(stop.Customer);
                unassigned.Remove(stop.Customer);
            }
        }

        if (route is {IsFinished: false})
            route.Seal();

        return solution;
    }
}
