using System.Numerics;
using System.Runtime.CompilerServices;
using Solvers.Types;

namespace Solvers;

public class Solver : ISolver
{
    public int? Seed { get; set; }
    public ISolver InitialSolver { get; set; } = new GreedySolver();
    public int Tenure { get; set; } = 10;

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        var rnd = Seed is null
            ? new Random()
            : new Random(Seed.Value);

        var initial = await InitialSolver.SolveFinalAsync(instance, stoppingToken);
        var incumbent = initial;
        var previous = initial;
        var tabu = new Queue<Customer>();

        yield return incumbent;

        while (!stoppingToken.IsCancellationRequested)
        {
            (Customer Customer, float Distance) candidate = (null!, 0);
            foreach (var route in previous.Routes)
            {
                var stops = route.Stops;
                if (stops.Count == 3)
                {
                    if (!tabu.Contains(stops[1].Customer))
                    {
                        candidate = (stops[1].Customer, 0);
                        break;
                    }
                }

                for (int i = 1; i < stops.Count - 2; i++)
                {
                    var currentStop = stops[i];
                    var nextStop = stops[i + 1];
                    if (tabu.Contains(nextStop.Customer))
                        continue;

                    var distance = nextStop.Distance - currentStop.Distance;
                    if (distance > candidate.Distance)
                        candidate = (nextStop.Customer, distance);
                }
            }

            tabu.Enqueue(candidate.Customer);
            if (tabu.Count > Tenure)
                tabu.Dequeue();

            // ReSharper disable once ReplaceWithSingleCallToFirst
            var neighbors = instance.Customers
                .OrderBy(x =>
                    Vector2.Distance(candidate.Customer.Position, x.Position) +
                    Math.Abs(candidate.Customer.ReadyTime - x.ReadyTime))
                .Where(x => x != candidate.Customer);


            foreach (var neighbor in neighbors)
            {
                if (!previous.TryMove(candidate.Customer, neighbor, out var current))
                    continue;


                if (current.Distance < incumbent.Distance || current.Routes.Count < incumbent.Routes.Count)
                {
                    yield return incumbent = current;
                    tabu.Clear();
                }

                previous = current;
                break;
            }

        }
    }
}
