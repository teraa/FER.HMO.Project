namespace Solvers;

public interface ISolver
{
    Solution Solve(Instance instance);
}

public class Solver : ISolver
{
    public Solution Solve(Instance instance)
    {
        throw new NotImplementedException();
    }
}
