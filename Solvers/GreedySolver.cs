using System.Diagnostics;

namespace Solvers;

public class GreedySolver : ISolver
{
    public Solution Solve(Instance instance)
    {
        var solution = new Solution();
        var unassigned = instance.Customers.ToList();
        var route = null as Route;

        while (unassigned.Any())
        {
            if (route is null)
            {
                route = new Route(instance.Depot, instance.Capacity);
                solution.Routes.Add(route);
            }

            var eligible = unassigned
                .Select(x => (customer: x, isEligible: route.CanAdd(x, out var serviceStartTime), serviceStartTime))
                .Where(x => x.isEligible)
                .OrderBy(x => x.serviceStartTime)
                .ToList();

            var (customer, _, _) = eligible
                .FirstOrDefault();

            if (customer is null)
            {
                // return to depot and start a new route
                Debug.Assert(solution.Routes.Count < instance.Vehicles);
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
