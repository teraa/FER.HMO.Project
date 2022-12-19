namespace Solvers;

public interface ISolver
{
    Solution Solve(Instance instance);
}

public class Solver : ISolver
{
    public ISolver InitialSolver { get; set; } = new GreedySolver();

    public Solution Solve(Instance instance)
    {
        var initial = InitialSolver.Solve(instance);

        return initial;
    }
}
