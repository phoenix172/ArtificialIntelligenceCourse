using System.Collections.Concurrent;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;

namespace GeneticAlgorithms;

public static class TravelingSalesman
{
    public static async Task Run()
    {
        //await Run(CostMatrix5);
    }

    public static async Task<Route> Run(int[][] costMatrix, int populationSize = 1000, double mutationRate = 0.000001, int stabilizationThreshold = 1000, int threadCount = 16)
    {
        int chromosomeLength = costMatrix.GetLength(0);
        var genetic = GeneticAlgorithm<int>.Create(populationSize,
            random => Route.CreateRandom(chromosomeLength, costMatrix, random));
        genetic.ThreadCount = threadCount;
        genetic.MutationRate = mutationRate;
        genetic.StabilizationThreshold = stabilizationThreshold;

        var timer = Stopwatch.StartNew();

        Route? best = await genetic.Compute(context =>
        {
            if (context.IterationNumber % 1000 != 0) return;
            var first = context.Pool.First();
            Console.WriteLine($"Iteration {context.IterationNumber}: {first}");
        }) as Route;

        timer.Stop();

        Console.WriteLine();

        Console.WriteLine($"one solution:");
        Console.WriteLine(best);

        Console.WriteLine();

        Console.WriteLine($"all solutions:");
        genetic.OptimalSolutions.ToList().ForEach(Console.WriteLine);

        Console.WriteLine();

        Console.WriteLine($"answer found in {timer.ElapsedMilliseconds} ms");

        return best;
    }
}