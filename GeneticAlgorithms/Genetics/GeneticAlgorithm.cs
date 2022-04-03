using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GeneticAlgorithms.Genetics;

public class GeneticAlgorithm<TGene>
{
    private const double Epsilon = 0.0000000001;

    private volatile int _stabilization = 0;

    private readonly int _populationSize;
    private IPopulation<TGene> _population;
    private CancellationTokenSource _tokenSource;
    private ConcurrentDictionary<WeightedChromosome<TGene>, object?> _optimalSolutions = new();

    private GeneticAlgorithm(int populationSize, object? target, double targetFitness, ChromosomeFactory<TGene> chromosomeFactory, double mutationRate = 0.001d)
    {
        Target = target;
        TargetFitness = targetFitness;
        MutationRate = mutationRate;
        _populationSize = populationSize;

        _population = Population<TGene>
            .CreateRandom(populationSize, chromosomeFactory, new Random());
    }

    public double TargetFitness { get; set; }
    public object? Target { get; set; }
    public double MutationRate { get; set; }
    public int ThreadCount { get; set; } = 1;
    public int StabilizationThreshold { get; set; } = 1000;
    public WeightedChromosome<TGene>? CurrentBest { get; private set; }
    public IEnumerable<WeightedChromosome<TGene>> OptimalSolutions => _optimalSolutions.Keys;

    public record GenerationContext(BreedingPool<TGene> Pool, int IterationNumber);

    public static GeneticAlgorithm<TGene> Create(
        int populationSize,
        ChromosomeFactory<TGene> chromosomeFactory,
        object? target = null,
        double targetFitness = double.MaxValue)
    {
        return new GeneticAlgorithm<TGene>(populationSize, target, targetFitness, chromosomeFactory);
    }

    public Task<IChromosome<TGene>?> Compute(Action<GenerationContext>? action = null, CancellationToken token = default)
    {
        int iterationCount = 0;

        Random globalRandom = new Random(DateTime.UtcNow.GetHashCode());

        List<Thread> workers = new();
        ConcurrentBag<GenerationResult> generations = new();

        TaskCompletionSource<IChromosome<TGene>?> tcs = new();
        _tokenSource = new();

        Barrier barrier = new Barrier(ThreadCount, _ =>
        {
            if (generations.FirstOrDefault(x => x.Result != null)?.Result is { } result)
            {
                _tokenSource.Cancel();
                tcs.TrySetResult(result);
            }

            Debug.Assert(generations.Count == ThreadCount);
            MergeAndPrune(generations);
            generations.Clear();

            if (token.IsCancellationRequested || _tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                tcs.TrySetResult(CurrentBest?.Chromosome);
            }
        });

        for (int i = 0; i < ThreadCount; i++)
        {
            Thread worker = new Thread(() =>
            {
                int threadNumber = i;
                Random random =
                    new Random(DateTime.UtcNow.GetHashCode() + threadNumber + globalRandom.Next(0, 1000));
                while (!_tokenSource.IsCancellationRequested)
                {
                    var newPopulation = ComputeOne(_population, action, Interlocked.Increment(ref iterationCount), random);
                    generations.Add(newPopulation);
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
        var selected = newChromosomes.Take(_populationSize).ToList();
        UpdateCurrentBest(selected.First());
        _population = new Population<TGene>(_populationSize, selected.Select(x => x.Chromosome), random);
    }

    private GenerationResult ComputeOne(IPopulation<TGene> population, Action<GenerationContext>? action, int iterationNumber, Random random)
    {
        var pool = population.SelectBreedingPool(Target);

        action?.Invoke(new(pool, iterationNumber));

        bool success = Evaluate(pool, out var result);
        if (success)
        {
            return new(population, result);
        }

        var newPopulation = population.Crossover(pool, random);
        newPopulation.Mutate(MutationRate);

        return new(newPopulation, result);
    }

    private void UpdateCurrentBest(WeightedChromosome<TGene> best)
    {
        double delta = Math.Abs(best.Fitness - CurrentBest?.Fitness ?? 0);
        if (delta < Epsilon)
        {
            Interlocked.Increment(ref _stabilization);
            _optimalSolutions.TryAdd(best, null);
        }
        else
        {
            Interlocked.Exchange(ref _stabilization, 0);
            _optimalSolutions.Clear();
        }

        if (_stabilization >= StabilizationThreshold) _tokenSource.Cancel();

        CurrentBest = best;
    }

    private bool Evaluate(BreedingPool<TGene> pool, [NotNullWhen(true)] out IChromosome<TGene>? result)
    {
        var best = pool.First();
        if (best.Fitness >= TargetFitness)
        {
            result = best.Chromosome;
            return true;
        }

        result = null;
        return false;
    }
}