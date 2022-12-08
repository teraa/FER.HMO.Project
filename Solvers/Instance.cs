namespace Solvers;

public record Instance(
    int Vehicles,
    int Capacity,
    Position StartingPosition,
    int DueDate,
    IReadOnlyList<Customer> Customers);

public record Customer(
    int Id,
    Position Position,
    int Demand,
    int ReadyTime,
    int DueDate,
    int ServiceTime);

public record Position(int X, int Y);
