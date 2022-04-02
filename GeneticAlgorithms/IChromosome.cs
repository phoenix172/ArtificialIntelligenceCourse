namespace ArtificialIntelligenceCourse;

public interface IChromosome<TGene>
{
    double Fitness(IChromosome<TGene> target);
    IChromosome<TGene> Crossover(IChromosome<TGene> other, Random random);
    void Mutate(double mutationRate, Random random);
}