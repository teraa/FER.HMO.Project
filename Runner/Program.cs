using Solvers;

var instance = InstanceLoader.LoadFromFile("../instances/i1.txt");
var solver = new Solver();
await foreach (var solution in solver.SolveAsync(instance))
{
    Console.WriteLine(solution);
}
