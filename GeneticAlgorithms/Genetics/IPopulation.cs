namespace GeneticAlgorithms.Genetics;

public interface IPopulation<TGene> : IEnumerable<IChromosome<TGene>>
{
    Random Random { get; }
    BreedingPool<TGene> SelectBreedingPool(object? target);
    IPopulation<TGene> Crossover(List<WeightedChromosome<TGene>> breedingPool, Random? random);
    void Mutate(double mutationRate);
    void Add(IPopulation<TGene> newPopulation);
}