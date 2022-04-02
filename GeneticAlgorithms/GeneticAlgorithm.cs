using System.Diagnostics.CodeAnalysis;
using ArtificialIntelligenceCourse;

namespace GeneticAlgorithms;

public class ThreadLocalRandom
{
    readonly ThreadLocal<Random> random =
        new ThreadLocal<Random>(() => new Random(GetSeed()));

    int Rand()
    {
        return random.Value.Next();
    }

    static int GetSeed()
    {
        return Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
    }

    public Random Instance => random.Value;
}

public class GeneticAlgorithm<TGene>
{
    
    private readonly ThreadLocalRandom _random = new();

    private readonly int _populationSize;
    private readonly ChromosomeFactory<TGene> _chromosomeFactory;
    private IPopulation<TGene> _population;

    private GeneticAlgorithm(int populationSize, IChromosome<TGene> target, double targetFitness, ChromosomeFactory<TGene> chromosomeFactory, double mutationRate = 0.001d)
    {
        Target = target;
        TargetFitness = targetFitness;
        MutationRate = mutationRate;
        _populationSize = populationSize;
        _chromosomeFactory = chromosomeFactory;
        _population = Population<TGene>
            .CreateRandom(populationSize, chromosomeFactory, _random.Instance);
    }

    public double TargetFitness { get; set; }
    public IChromosome<TGene> Target { get; set; }
    public double MutationRate { get; set; }
    public int ThreadCount { get; set; } = 10;

    public record GenerationContext(BreedingPool<TGene> Pool, int IterationNumber);

    public static GeneticAlgorithm<TGene> Create(
        int initialPopulationSize,
        IChromosome<TGene> target,
        ChromosomeFactory<TGene> chromosomeFactory)
    {
        return new GeneticAlgorithm<TGene>(initialPopulationSize, target, target.Fitness(target), chromosomeFactory);
    }

    public async Task<IChromosome<TGene>> Compute(Action<GenerationContext>? action = null)
    {
        int iterationCount = 0;

        var randoms = Enumerable.Range(1, ThreadCount).Select(x =>
        {
            Thread.Sleep(x+7);
            return new Random(x+DateTime.Now.Ticks.GetHashCode());
        }).ToArray();

        while (true)
        {
            var tasks = Enumerable.Range(1, ThreadCount).Select(i => Task.Run(() =>
            {
                var newPopulation= ComputeOne(_population, action, Interlocked.Increment(ref iterationCount), randoms[i-1]);
                return newPopulation;
            }));

            var newPopulations = await Task.WhenAll(tasks);

            (_, IChromosome<TGene>? Result) = newPopulations.FirstOrDefault(x => x.Result != null);
            if (Result is not null) return Result;

            var newChromosomes = newPopulations.SelectMany(x => x.Population.SelectBreedingPool(Target)).Take(_populationSize).Select(x=>x.Chromosome).ToList();
            _population = new Population<TGene>(_populationSize, newChromosomes, newPopulations.First().Population.Random);
        }
    }

    private (IPopulation<TGene> Population, IChromosome<TGene>? Result) ComputeOne(IPopulation<TGene> population, Action<GenerationContext>? action, int iterationNumber, Random random)
    {
        var pool = population.SelectBreedingPool(Target);

        action?.Invoke(new(pool, iterationNumber));

        bool success = Evaluate(pool, out var result);
        if (success)
        {
            return (population, result);
        }

        Console.WriteLine(_random.Instance.GetHashCode());
        var newPopulation = population.Crossover(pool, random);
        newPopulation.Mutate(MutationRate);
        
        return (newPopulation, result);
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