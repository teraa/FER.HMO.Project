using System.Diagnostics;
using Solvers;
using Timer = System.Timers.Timer;
// ReSharper disable AccessToModifiedClosure

const string inFile = "../instances/i1.txt";
const string outDir = "../out/";
Directory.CreateDirectory(outDir);
var instance = InstanceLoader.LoadFromFile(inFile);
var solver = new Solver();
var incumbent = null as Solution;

foreach (var time in new[] {1, 5})
{
    var timer = new Timer(TimeSpan.FromMinutes(time))
    {
        AutoReset = false,
        Enabled = true
    };
    timer.Elapsed += (_, _) => WriteToFile($"{time}m", incumbent!);
}

int i = 0;
var sw = Stopwatch.StartNew();
await foreach (var solution in solver.SolveAsync(instance))
{
    incumbent = solution;
    Console.WriteLine($"Solution {i++}: {sw.Elapsed}");
    Console.WriteLine(solution);
}

WriteToFile("un", incumbent!);

void WriteToFile(string name, Solution solution)
{
    var outFile = Path.Combine(outDir, $"res-{name}-{Path.GetFileName(inFile)}");
    File.WriteAllText(outFile, solution.ToString());
}
