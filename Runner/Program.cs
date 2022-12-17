using Solvers;

var instance = InstanceLoader.LoadFromFile("../instances/i1.txt");
var solver = new Solver();
var solution = solver.Solve(instance);
Console.WriteLine(solution);
