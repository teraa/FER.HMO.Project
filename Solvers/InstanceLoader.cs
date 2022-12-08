namespace Solvers;

public static class InstanceLoader
{
    public static Instance LoadFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        const int l1 = 2, l2 = 7, l3 = 8;
        var vehicles = int.Parse(lines[l1][0]);
        var capacity = int.Parse(lines[l1][1]);
        var startingPosition = new Position(
            int.Parse(lines[l2][1]),
            int.Parse(lines[l2][2]));
        var dueDate = int.Parse(lines[l2][5]);

        var customers = new List<Customer>();
        for (int i = l3; i < lines.Length; i++)
        {
            var parts = lines[i]
                .Select(int.Parse)
                .ToArray();

            int part = 0;
            var customer = new Customer(
                Id: parts[part++],
                Position: new Position(
                    X: parts[part++],
                    Y: parts[part++]),
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
