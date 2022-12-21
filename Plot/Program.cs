using Solvers;

var instance = InstanceLoader.LoadFromFile(args[0]);
var points = instance.Customers
    .Select(x => x.Position)
    .OrderBy(p => p.X)
    .ThenBy(p => p.Y);

foreach (var point in points)
    Console.WriteLine($"{point.X};{point.Y}");
