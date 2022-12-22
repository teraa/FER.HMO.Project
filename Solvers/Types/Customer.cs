using System.Numerics;

namespace Solvers.Types;

public record Customer(
    int Id,
    Vector2 Position,
    int Demand,
    int ReadyTime,
    int DueTime,
    int ServiceTime)
{
    public virtual bool Equals(Customer? other)
        => ReferenceEquals(this, other);

    public override int GetHashCode()
        => HashCode.Combine(Id, Position, Demand, ReadyTime, DueTime, ServiceTime);
}
