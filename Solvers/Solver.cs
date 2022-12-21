using System.Runtime.CompilerServices;

namespace Solvers;

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
