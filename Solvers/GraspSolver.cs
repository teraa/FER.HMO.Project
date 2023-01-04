using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Solvers.Types;

namespace Solvers;

public class GraspSolver : ISolver
{
    public Func<Stop, double> ValueFunc { get; set; } = x => x.ServiceStartedAt;
    public int? Seed { get; set; }
    public int Iterations { get; set; } = 10;
    public int MaxRclSize { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        var rnd = Seed is null
            ? new Random()
            : new Random(Seed.Value);

        var channel = Channel.CreateUnbounded<Solution>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false,
        });

        for (int i = 0; i <= MaxRclSize; i++)
        {
            _ = SolveInternalAsync(instance, rnd, channel.Writer, i, stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            Solution solution;
            try
            {
                solution = await channel.Reader.ReadAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            yield return solution;
        }
    }

    private async Task SolveInternalAsync(Instance instance, Random rnd, ChannelWriter<Solution> writer, int rlcSize,
        CancellationToken stoppingToken)
    {
        await Task.Yield();

        var incumbent = null as Solution;

        for (int j = 0; j < Iterations; j++)
        {
            var current = Construct(instance, rnd, rlcSize);

            if (incumbent is { } && current >= incumbent)
                continue;

            try
            {
                await writer.WriteAsync(current, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            incumbent = current;
        }

        if (incumbent is null)
            return;

        TimeSpan? timeout = rlcSize == 0
            ? null
            : Timeout;

        foreach (var solution in Improve(incumbent, instance, rnd, timeout, stoppingToken))
        {
            try
            {
                await writer.WriteAsync(solution, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private IEnumerable<Solution> Improve(Solution incumbent, Instance instance, Random rnd, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var previous = incumbent;

        ulong i = 0;
        ulong j = 0;
        while (!cancellationToken.IsCancellationRequested && (timeout is null || sw.Elapsed < timeout))
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
                sw.Restart();
                Debug.WriteLine($"[SOLVER] Improved at {j}");
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
