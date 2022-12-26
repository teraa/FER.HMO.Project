using System.Diagnostics;
using Solvers;
using Solvers.Types;
using Timer = System.Timers.Timer;

// ReSharper disable AccessToModifiedClosure

const string inFile = "../instances/i1.txt";
const string outDir = "../out/";
Directory.CreateDirectory(outDir);
var instance = InstanceLoader.LoadFromFile(inFile);
var solver = new RandomMoveSolver()
{
    InitialSolver = new GreedySolver()
    {
        ValueFunc = x => -x.ServiceStartedAt / x.Distance,
    }
};
var incumbent = null as Solution;
var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

foreach (var time in new[] {1, 5})
{
    var timer = new Timer(TimeSpan.FromMinutes(time))
    {
        AutoReset = false,
        // Enabled = true
    };
    timer.Elapsed += (_, _) => WriteToFile($"{time}m", incumbent!);
}

int i = 0;
var sw = Stopwatch.StartNew();
await foreach (var solution in solver.SolveAsync(instance, cts.Token))
{
    incumbent = solution;
    Console.WriteLine($"[{sw.Elapsed:hh\\:mm\\:ss}] Solution {i++}: {solution.Routes.Count}/{solution.Vehicles} ({solution.Distance:F})");
}

WriteToFile("un", incumbent!);

void WriteToFile(string name, Solution solution)
{
    var outFile = Path.Combine(outDir, $"res-{name}-{Path.GetFileName(inFile)}");
    File.WriteAllText(outFile, solution.ToString());
}
