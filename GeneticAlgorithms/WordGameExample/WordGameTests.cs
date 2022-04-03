using NUnit.Framework;

namespace GeneticAlgorithms.WordGameExample;

[TestFixture]
public class WordGameTests
{
    [TestCase("Pesho e gosho")]
    [TestCase("How to make a distance matrix for traveling salesman")]
    [TestCase("I am trying to develop a program in C++ from Travelling Salesman Problem Algorithm. I need a distance matrix and a cost matrix")]
    public async Task Run_ReturnsCorrectResult(string target)
    {
        var result = await WordGame.Run(target, target);

        Assert.AreEqual(result, target);
    }
}