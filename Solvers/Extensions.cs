namespace Solvers;

public static class Extensions
{
    public static T Random<T>(this IReadOnlyList<T> list, Random random)
        => list[random.Next(0, list.Count)];
}
