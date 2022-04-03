namespace GeneticAlgorithms.Genetics;

public record WeightedChromosome<TGene>(IChromosome<TGene> Chromosome, double Fitness)
{
    private double? _normalizedFitness;
    public void Normalize(double ratio) => NormalizedFitness = Fitness * ratio;

    public override string ToString() => $"{nameof(Chromosome)}: {Chromosome}, {nameof(Fitness)}: {Fitness}";

    public double NormalizedFitness
    {
        get => _normalizedFitness ?? throw new InvalidOperationException($"WeightedChromosome has not been Normalized: {this}");
        private set => _normalizedFitness = value;
    }
};