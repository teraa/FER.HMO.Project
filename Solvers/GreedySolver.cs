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
                .Select(x => (
                    Customer: x,
                    IsEligible: route.CanAdd(x, out var serviceStartTime),
                    ServiceStartTime: serviceStartTime
                ))
                .Where(x => x.IsEligible)
                .OrderBy(x => x.ServiceStartTime);

            var (customer, _, _) = eligible.FirstOrDefault();

            if (customer is null)
            {
                // return to depot and start a new route
                route.Add(route.Depot);
                route = null;
            }
            else
            {
                route.Add(customer);
                unassigned.Remove(customer);
            }
        }

        if (route is {IsFinished: false})
            route.Add(route.Depot);

        return solution;
    }
}
