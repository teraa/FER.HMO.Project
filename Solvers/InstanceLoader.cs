using System.Numerics;

namespace Solvers;

public static class InstanceLoader
{
    public static Instance LoadFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        int i = 2;
        var vehicles = int.Parse(lines[i][0]);
        var capacity = int.Parse(lines[i][1]);

        i = 7;
        var startingPosition = new Vector2(
            x: int.Parse(lines[i][1]),
            y: int.Parse(lines[i][2]));
        var dueDate = int.Parse(lines[i][5]);

        var customers = new List<Customer>();
        for (i = 8; i < lines.Length; i++)
        {
            var parts = lines[i]
                .Select(int.Parse)
                .ToArray();

            int part = 0;
            var customer = new Customer(
                Id: parts[part++],
                Position: new Vector2(
                    x: parts[part++],
                    y: parts[part++]),
                Demand: parts[part++],
                ReadyTime: parts[part++],
                DueDate: parts[part++],
                ServiceTime: parts[part]);

            customers.Add(customer);
        }

        return new Instance(
            vehicles,
            capacity,
            startingPosition,
            dueDate,
            customers);
    }
}
