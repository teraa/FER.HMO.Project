using System.Numerics;

namespace Solvers;

public record Customer(
    int Id,
    Vector2 Position,
    int Demand,
    int ReadyTime,
    int DueTime,
    int ServiceTime);
