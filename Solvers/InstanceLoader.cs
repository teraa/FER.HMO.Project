namespace Solvers;

public static class InstanceLoader
{
    public static Instance LoadFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
            .Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        const int i1 = 2, i2 = 7, i3 = 8;
        var vehicles = int.Parse(lines[i1][0]);
        var capacity = int.Parse(lines[i1][1]);
        var startingPoint = new Point(
            int.Parse(lines[i2][1]),
            int.Parse(lines[i2][2]));
        var dueDate = int.Parse(lines[i2][5]);

        var customers = new List<Customer>();
        for (int i = i3; i < lines.Length; i++)
        {
            var parts = lines[i]
                .Select(int.Parse)
                .ToArray();

            int part = 0;
            var customer = new Customer(
                Id: parts[part++],
                Point: new Point(
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
            startingPoint,
            dueDate,
            customers);
    }
}
