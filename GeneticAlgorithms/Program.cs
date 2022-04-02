using GeneticAlgorithms;

var random = new Random(DateTime.Now.ToString().GetHashCode());

string target = @"Artificial Intelligence";

var alphabet = new string(Enumerable.Range('a', 'z' - 'a' + 1).Concat(Enumerable.Range('A', 'Z' - 'A' + 1)).Select(x => (char)x).Append(' ').ToArray());
var targetWord = new Word(target, Word.GenerateAlphabet(target));

var genetic = GeneticAlgorithm<char>.Create(1000, targetWord, ()=>targetWord.CreateRandom(target.Length, random));
genetic.MutationRate = 0.01;
genetic.ThreadCount = 1;

int iterationsCount = 0;
var result = await genetic.Compute(p =>
{
    Console.WriteLine($"Iteration number {p.IterationNumber} (Fitness Threshold: {genetic.TargetFitness}):");
    Console.WriteLine(p.Pool.First());
    Console.WriteLine("----------------------------------------");
    iterationsCount = p.IterationNumber;
});
Console.WriteLine($"Success: {result} in {iterationsCount} iterations");