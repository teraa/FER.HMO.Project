using System.Numerics;

namespace Solvers;

public record Instance(
    int Vehicles,
    int Capacity,
    Vector2 StartingPosition,
    int DueDate,
    IReadOnlyList<Customer> Customers);

public record Customer(
    int Id,
    Vector2 Position,
    int Demand,
    int ReadyTime,
    int DueDate,
    int ServiceTime);

public record Point(int X, int Y);
