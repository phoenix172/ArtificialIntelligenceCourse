using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GeneticAlgorithms
{
    [TestFixture]
    public class TravelingSalesmanTests
    {
        [TestCaseSource(typeof(TravelingSalespersonTestData), nameof(TravelingSalespersonTestData.AllTests))]
        public async Task Problem_ReturnsCorrectResult(TravelingSalespersonTestData testData)
        {
            var result = await TravelingSalesman.Run(testData.CostMatrix, stabilizationThreshold: 1000, populationSize: 100);

            Assert.AreEqual(testData.OptimalCost, result.Cost());
        }
    }
}
