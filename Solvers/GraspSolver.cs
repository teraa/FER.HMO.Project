using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class GraspSolver : ISolver
{
    public Func<Stop, double> ValueFunc { get; set; } = x => x.ServiceStartedAt;
    public int? Seed { get; set; }

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        await Task.Yield();

        var rnd = Seed is null
            ? new Random()
            : new Random(Seed.Value);

        var solution = Construct(instance, rnd, 10);

        yield return solution;
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
