using System.Numerics;
using Solvers.Types;

namespace Tests;

public static class Data
{
    public static readonly Customer Depot = new(0, Vector2.Zero, 0, 0, 0, 0);
    public static readonly Customer Customer = Depot with {Id = 1};
}
