namespace Solvers;

public record Instance(
    int Vehicles,
    int Capacity,
    Point StartingPoint,
    int DueDate,
    IReadOnlyList<Customer> Customers);

public record Customer(
    int Id,
    Point Point,
    int Demand,
    int ReadyTime,
    int DueDate,
    int ServiceTime);

public record Point(int X, int Y);
