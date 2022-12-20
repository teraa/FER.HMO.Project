using System.Runtime.CompilerServices;

namespace Solvers;

public interface ISolver
{
    IAsyncEnumerable<Solution> SolveAsync(Instance instance, CancellationToken stoppingToken = default);

    async Task<Solution> SolveFinalAsync(Instance instance, CancellationToken stoppingToken = default)
    {
        var latest = null as Solution;

        await foreach (var solution in SolveAsync(instance, stoppingToken))
        {
            latest = solution;
        }

        return latest!;
    }
}

public class Solver : ISolver
{
    public ISolver InitialSolver { get; set; } = new GreedySolver();

    public async IAsyncEnumerable<Solution> SolveAsync(Instance instance,
        [EnumeratorCancellation] CancellationToken stoppingToken = default)
    {
        var initial = await InitialSolver.SolveFinalAsync(instance, stoppingToken);

        yield return initial;
    }
}
