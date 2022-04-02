using ArtificialIntelligenceCourse;

namespace GeneticAlgorithms;

record Word(string Value, IReadOnlyList<char> Alphabet) : IChromosome<char>
{
    public static IReadOnlyList<char> GenerateAlphabet(string word) =>
        word.Distinct().ToArray();

    public Word CreateRandom(int length, Random random) 
        => new(new string(Enumerable.Range(1, length).Select(_ => NewChar(random)).ToArray()), Alphabet);

    private char NewChar(Random random) => Alphabet[random.Next(0,Alphabet.Count)];

    public double Fitness(IChromosome<char> target)
    {
        return Fitness(this, target as Word ?? throw new ArgumentException(nameof(target)));
    }

    public static double Fitness(Word left, Word right)
    {
        if(left.Value.Length != right.Value.Length)
            throw new ArgumentException();

        int correctScore = left.Value.Zip(right.Value).Count(x => x.First == x.Second);
        return Math.Pow(2, correctScore) + 0.01;
    }

    public IChromosome<char> Crossover(IChromosome<char> other, Random random)
    {
        if (other is not Word word) throw new ArgumentException(nameof(other));

        int splitPoint = random.Next(Value.Length);

        return new Word(Value[..splitPoint] + word.Value[splitPoint..], Alphabet);
    }

    public void Mutate(double mutationRate, Random random)
    {
        Value = new string(Value.Select(x => random.NextDouble() < mutationRate ? NewChar(random) : x).ToArray());
    }

    public string Value { get; private set; } = Value;

    public virtual bool Equals(Word? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public virtual bool Equals(IEnumerable<char>? other)
    {
        if (other == null) return false;
        return Value.SequenceEqual(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}