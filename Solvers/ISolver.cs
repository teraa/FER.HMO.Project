using Solvers.Types;

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
