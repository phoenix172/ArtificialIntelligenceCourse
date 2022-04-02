using System.Diagnostics;
using GeneticAlgorithms;
string target = @"Pesho e pulno govno dasdfi0adsjfasdnh0fjdasfjadsjfpas";

var alphabet = new string(Enumerable.Range('a', 'z' - 'a' + 1).Concat(Enumerable.Range('A', 'Z' - 'A' + 1)).Concat(Enumerable.Range('0',10)).Select(x => (char)x).Append(' ').ToArray());
var targetWord = new Word(target, Word.GenerateAlphabet(alphabet));

var genetic = GeneticAlgorithm<char>.Create(10000, targetWord, random=>targetWord.CreateRandom(target.Length, random));
genetic.MutationRate = 0.01;
genetic.ThreadCount = 16;

int iterationsCount = 0;
Stopwatch time = Stopwatch.StartNew();

var result = await genetic.Compute(p =>
{
    if (p.IterationNumber % 100 == 0)
    {
        Console.WriteLine($"Iteration number {p.IterationNumber} (Fitness Threshold: {genetic.TargetFitness}):");
        Console.WriteLine(p.Pool.First());
        Console.WriteLine("----------------------------------------");
    }
    
    iterationsCount = p.IterationNumber;
});

Console.WriteLine($"Success: `{result}` in {iterationsCount} iterations ({time.ElapsedMilliseconds} ms)");
