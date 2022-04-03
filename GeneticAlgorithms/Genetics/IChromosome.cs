namespace GeneticAlgorithms.Genetics;

public interface IChromosome<TGene>
{
    double Fitness(object? target);
    IChromosome<TGene> Crossover(IChromosome<TGene> other, Random random);
    void Mutate(double mutationRate, Random random);
}