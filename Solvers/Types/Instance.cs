namespace Solvers.Types;

public record Instance(
    int Vehicles,
    int Capacity,
    Customer Depot,
    IReadOnlyList<Customer> Customers);
