namespace Solvers;

public interface ISolver
{
    Task<Solution> SolveAsync(Instance instance, CancellationToken stoppingToken);
}

public class Solver : ISolver
{
    public ISolver InitialSolver { get; set; } = new GreedySolver();

    public Task<Solution> SolveAsync(Instance instance, CancellationToken stoppingToken = default)
    {
        var initial = InitialSolver.SolveAsync(instance, stoppingToken);

        return initial;
    }
}
