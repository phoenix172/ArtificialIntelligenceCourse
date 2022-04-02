using GeneticAlgorithms.Genetics;

namespace GeneticAlgorithms;

record RouteTarget(int Cost, int StartCity, int EndCity);

record Route(int[] RouteItems, int[][] CostMatrix) : IChromosome<int>
{
    public double Fitness(object target)
    {
        if (target is not RouteTarget routeTarget) throw new ArgumentException(nameof(target));

        double cost = (1 - Math.Abs(routeTarget.Cost - Cost()) / routeTarget.Cost) +
                 (routeTarget.StartCity == RouteItems[0] ? 1 : 0) +
                 (routeTarget.EndCity == RouteItems[^1] ? 1 : 0);

        return Math.Pow(2, cost);
    }

    public double Cost() =>
        RouteItems.Skip(1)
            .Aggregate((cost: 0, prev: RouteItems[0]),
                (result, x) => (cost: result.cost + CostMatrix[result.prev][x], prev: x)).cost;

    public IChromosome<int> Crossover(IChromosome<int> other, Random random)
    {
        if (other is not Route route || route.RouteItems.Length != RouteItems.Length) throw new ArgumentException(nameof(other));

        int index = random.Next(0, RouteItems.Length);

        return new Route(RouteItems[0..index].Concat(route.RouteItems[index..]).ToArray(), CostMatrix);
    }

    public void Mutate(double mutationRate, Random random)
    {
        if (random.NextDouble() < mutationRate) return;

        int indexA = random.Next(0,RouteItems.Length);
        int indexB = -1;
        do
        {
            indexB = random.Next(0, RouteItems.Length);
        } while (indexA == indexB);

        (RouteItems[indexA], RouteItems[indexB]) = (RouteItems[indexB], RouteItems[indexA]);
    }
}