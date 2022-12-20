using Solvers;

var instance = InstanceLoader.LoadFromFile("../instances/i1.txt");
var solver = new Solver();
var solution = await solver.SolveAsync(instance);
Console.WriteLine(solution);
