namespace GeneticAlgorithms.Genetics;

public class BreedingPool<TGene> : List<WeightedChromosome<TGene>>
{
    private BreedingPool(IEnumerable<WeightedChromosome<TGene>> source)
        :base(source.ToList())
    {
    }

    public static BreedingPool<TGene> Create(IEnumerable<IChromosome<TGene>> source, object? target)
    {
        var sourcePool = source.Select(x => new WeightedChromosome<TGene>(x, x.Fitness(target))).ToList();
        var ratio = 100.0 / sourcePool.Max(x => x.Fitness);
        sourcePool.ForEach(x => x.Normalize(ratio));

        var breedingPool = new BreedingPool<TGene>(sourcePool.OrderByDescending(x => x.NormalizedFitness));
        return breedingPool;
    }
};