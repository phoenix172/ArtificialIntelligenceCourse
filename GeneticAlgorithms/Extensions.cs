namespace GeneticAlgorithms;

public static class Extensions
{
    public static IEnumerable<double> Normalized<T>(this IEnumerable<T> source, Func<T, double> key)
    {
        //negative numbers wont work
        var ratio = 100.0 /  source.Max(key);
        var normalizedList = source.Select(i => key(i) * ratio);
        return normalizedList;
    }
}