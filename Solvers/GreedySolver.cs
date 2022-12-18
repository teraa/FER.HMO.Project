namespace Solvers;

public class GreedySolver : ISolver
{
    public Solution Solve(Instance instance)
    {
        var solution = new Solution(instance.Vehicles);
        var unassigned = instance.Customers.ToList();
        var route = null as Route;

        while (unassigned.Any())
        {
            route ??= solution.CreateRoute(instance.Depot, instance.Capacity);

            // ReSharper disable once AccessToModifiedClosure
            var eligible = unassigned
                .Select(x => (customer: x, isEligible: route.CanAdd(x, out var serviceStartTime), serviceStartTime))
                .Where(x => x.isEligible)
                .OrderBy(x => x.serviceStartTime);

            var (customer, _, _) = eligible
                .FirstOrDefault();

            if (customer is null)
            {
                // return to depot and start a new route
                route.Add(instance.Depot);
                route = null;
            }
            else
            {
                route.Add(customer);
                unassigned.Remove(customer);
            }
        }

        return solution;
    }
}
