using System.Collections.Concurrent;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;

namespace GeneticAlgorithms;

public static class TravelingSalesperson
{
    private static readonly RouteTarget Target = new(Cost: 40, StartCity: 5, EndCity: 3);
    private static readonly int[][] CostMatrix =
    {
        new[] {0, 36, 32, 54, 20, 40},
        new[] {36, 0, 22, 58, 54, 67},
        new[] {32, 22, 0, 36, 42, 71},
        new[] {54, 58, 36, 0, 50, 92},
        new[] {20, 54, 42, 50, 0, 45},
        new[] {40, 67, 71, 92, 45, 0}
    };

    private const double Epsilon = 0.0001;
    private const int StabilizationThreshold = 1000;

    public static async Task Run()
    {
        var genetic = GeneticAlgorithm<int>.Create(100, Target, 7.5,
            random => Route.CreateRandom(CostMatrix.GetLength(0), CostMatrix, random));
        genetic.ThreadCount = 16;
        genetic.MutationRate = Epsilon;

        WeightedChromosome<int>? currentBest = null;

        CancellationTokenSource cts = new();

        int stabilization = 0;
        ConcurrentDictionary<Route, object?> optimalSolutions = new();

        var timer = Stopwatch.StartNew();

        Route best = (Route) await genetic.Compute(context =>
        {
            var first = context.Pool.First();
            if (Math.Abs(first.Fitness - currentBest?.Fitness ?? 0) < Epsilon)
            {
                stabilization++;
                optimalSolutions.TryAdd((Route)first.Chromosome, null);
            }
            else
            {
                stabilization = 0;
                optimalSolutions.Clear();
            }

            if(stabilization >= StabilizationThreshold) cts.Cancel();

            if (context.IterationNumber % 1000 != 0) return;
            Console.WriteLine($"Iteration {context.IterationNumber}: {first}");
        }, cts.Token);

        timer.Stop();

        Console.WriteLine();

        Console.WriteLine($"one solution:");
        Console.WriteLine(best);

        Console.WriteLine();

        Console.WriteLine($"all solutions:");
        optimalSolutions.Keys.ToList().ForEach(Console.WriteLine);

        Console.WriteLine();

        Console.WriteLine($"answer found in {timer.ElapsedMilliseconds} ms");
    }

}