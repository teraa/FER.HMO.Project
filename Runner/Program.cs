using System.Diagnostics;
using Solvers;
using Solvers.Types;
using Timer = System.Timers.Timer;

// ReSharper disable AccessToModifiedClosure

const string inFile = "../instances/i1.txt";
// string inFile = args[0];
const string outDir = "../solutions/";
Directory.CreateDirectory(outDir);
var instance = InstanceLoader.LoadFromFile(inFile);
var solver = new GraspSolver();
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
var cts = new CancellationTokenSource();
var sw = Stopwatch.StartNew();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;

    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
        cts.Cancel();
    else
        Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss}] Solution {i} ({solver.Iteration}):\n{incumbent}\n");
};

await foreach (var solution in solver.SolveAsync(instance, cts.Token))
{
    if (incumbent is { } && solution >= incumbent)
        continue;

    incumbent = solution;
    Console.WriteLine($"[{sw.Elapsed:hh\\:mm\\:ss}] Solution {i++} ({solver.Iteration}): {solution.Routes.Count}/{solution.Vehicles} ({solution.Distance:F})");
}

WriteToFile("un", incumbent!);

void WriteToFile(string name, Solution solution)
{
    var outFile = Path.Combine(outDir, $"res-{name}-{Path.GetFileName(inFile)}");
    File.WriteAllText(outFile, solution.ToString() + "\n" + solver.Iteration);
}
