using System.Numerics;
using Solvers.Types;

namespace Tests;

public static class Data
{
    public static Customer Depot => new(0, Vector2.Zero, 0, 0, 0, 0);
    public static Customer Customer => Depot with {Id = 1};
}
