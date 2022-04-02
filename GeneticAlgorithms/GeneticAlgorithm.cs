using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ArtificialIntelligenceCourse;
using Microsoft.VisualBasic;

namespace GeneticAlgorithms;

public class GeneticAlgorithm<TGene>
{
    private readonly int _populationSize;
    private IPopulation<TGene> _population;

    private GeneticAlgorithm(int populationSize, IChromosome<TGene> target, double targetFitness, ChromosomeFactory<TGene> chromosomeFactory, double mutationRate = 0.001d)
    {
        Target = target;
        TargetFitness = targetFitness;
        MutationRate = mutationRate;
        _populationSize = populationSize;

        _population = Population<TGene>
            .CreateRandom(populationSize, chromosomeFactory, new Random());
    }

    public double TargetFitness { get; set; }
    public IChromosome<TGene> Target { get; set; }
    public double MutationRate { get; set; }
    public int ThreadCount { get; set; } = 1;

    public record GenerationContext(BreedingPool<TGene> Pool, int IterationNumber);

    public static GeneticAlgorithm<TGene> Create(
        int initialPopulationSize,
        IChromosome<TGene> target,
        ChromosomeFactory<TGene> chromosomeFactory)
    {
        return new GeneticAlgorithm<TGene>(initialPopulationSize, target, target.Fitness(target), chromosomeFactory);
    }

    public Task<IChromosome<TGene>> Compute(Action<GenerationContext>? action = null)
    {
        int iterationCount = 0;

        Random globalRandom = new Random(DateTime.UtcNow.GetHashCode());

        List<Thread> workers = new();
        ConcurrentBag<GenerationResult> generations = new();
        CancellationTokenSource cts = new();
        TaskCompletionSource<IChromosome<TGene>> tcs = new();

        Barrier barrier = new Barrier(ThreadCount, _ =>
        {
            if (generations.FirstOrDefault(x => x.Result != null)?.Result is { } result)
            {
                if (cts.IsCancellationRequested) return;
                cts.Cancel();
                tcs.SetResult(result);
            }
            Debug.Assert(generations.Count == ThreadCount);
            MergeAndPrune(generations);
            generations.Clear();
        });

        for (int i = 0; i < ThreadCount;i++)
        {
            Thread worker = new Thread(() =>
            {
                int threadNumber = i;
                Random random = new Random(DateTime.UtcNow.GetHashCode() + threadNumber + globalRandom.Next(0, 1000));
                while (!cts.IsCancellationRequested)
                {
                    var newPopulation = ComputeOne(_population, action, Interlocked.Increment(ref iterationCount),random);
                    generations.Add(newPopulation);
                    Console.WriteLine($"Thread { threadNumber} done");

                    barrier.SignalAndWait();
                }
            });
            workers.Add(worker);
            worker.Start();
        }

        return tcs.Task;
    }

    private record GenerationResult(IPopulation<TGene> Population, IChromosome<TGene>? Result);

    private void MergeAndPrune(IEnumerable<GenerationResult> newPopulations)
    {
        var newChromosomes = BreedingPool<TGene>.Create(newPopulations.SelectMany(x => x.Population), Target);
        var random = newPopulations.First().Population.Random;
        _population = new Population<TGene>(_populationSize, newChromosomes.Select(x => x.Chromosome).Take(_populationSize), random);
    }

    private GenerationResult ComputeOne(IPopulation<TGene> population, Action<GenerationContext>? action, int iterationNumber, Random random)
    {
        var pool = population.SelectBreedingPool(Target);

        action?.Invoke(new(pool, iterationNumber));

        bool success = Evaluate(pool, out var result);
        if (success)
        {
            return new (population, result);
        }

        var newPopulation = population.Crossover(pool, random);
        newPopulation.Mutate(MutationRate);
        
        return new (newPopulation, result);
    }

    private bool Evaluate(BreedingPool<TGene> pool, [NotNullWhen(true)]out IChromosome<TGene>? result)
    {
        var best = pool.First();
        if (best.Chromosome.Equals(Target))
        {
            result = best.Chromosome;
            return true;
        }

        result = null;
        return false;
    }
}