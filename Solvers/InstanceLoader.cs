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

        var customers = new List<Customer>();
        for (i = 7; i < lines.Length; i++)
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
                DueTime: parts[part++],
                ServiceTime: parts[part]);

            customers.Add(customer);
        }

        var depot = customers[0];
        customers.Remove(depot);

        return new Instance(
            vehicles,
            capacity,
            depot,
            customers);
    }
}
