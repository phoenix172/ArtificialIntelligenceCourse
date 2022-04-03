using System.Collections;

namespace GeneticAlgorithms.Genetics;

public class Population<TGene> : IPopulation<TGene>
{
    public Random Random { get; }
    private IEnumerable<IChromosome<TGene>> _chromosomes;
    private int _populationCount = 0;

    public Population(int populationCount, IEnumerable<IChromosome<TGene>> chromosomes, Random random)
    {
        _populationCount = populationCount;
        _chromosomes = chromosomes;
        Random = random;
    }

    public static IPopulation<TGene> CreateRandom(int populationSize, ChromosomeFactory<TGene> factory, Random random)
    {
        var chromosomes = Enumerable
            .Range(1, populationSize)
            .Select(_ => factory(random))
            .ToList();
        var population = new Population<TGene>(populationSize, chromosomes, random);
        return population;
    }

    private IChromosome<TGene> SelectChromosome(List<WeightedChromosome<TGene>> pool)
    {
        WeightedChromosome<TGene> current = pool[Random.Next(_populationCount)];
        double maxFitness = current.NormalizedFitness;

        for (int i = 0;i<1000;i++)
        {
            current = pool[Random.Next(_populationCount)];
            if (Random.NextDouble()*maxFitness < current.NormalizedFitness)
            {
                return current.Chromosome;
            }
        }

        throw new InvalidOperationException("Selection failed");
    }

    public BreedingPool<TGene> SelectBreedingPool(object? target)
    {
        return BreedingPool<TGene>.Create(this, target);
    }

    public IPopulation<TGene> Crossover(List<WeightedChromosome<TGene>> breedingPool, Random? random = null)
    {
        random ??= Random;
        return new Population<TGene>(_populationCount, 
            Enumerable.Range(1, _populationCount / 2).SelectMany(_ => MakeBabies(breedingPool, random)).ToList(), random);
    }

    private IEnumerable<IChromosome<TGene>> MakeBabies(List<WeightedChromosome<TGene>> pool, Random random)
    {
        var parentA = SelectChromosome(pool);
        var parentB = SelectChromosome(pool);

        yield return parentA.Crossover(parentB, random);
        yield return parentB.Crossover(parentA, random);
    }

    public void Mutate(double mutationRate)
    {
        foreach (var chromosome in _chromosomes)
        {
            chromosome.Mutate(mutationRate, Random);
        }
    }

    public void Add(IPopulation<TGene> newPopulation)
    {
        _chromosomes = _chromosomes.Concat(newPopulation);
        _populationCount += newPopulation.Count();
    }

    public IEnumerator<IChromosome<TGene>> GetEnumerator()
    {
        return _chromosomes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _chromosomes).GetEnumerator();
    }
}