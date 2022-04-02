using System.Diagnostics;
using GeneticAlgorithms.Genetics;

namespace GeneticAlgorithms;

record RouteTarget(int Cost, int StartCity, int EndCity);

record Route(int[] RouteItems, int[][] CostMatrix) : IChromosome<int>
{
    private readonly ThreadLocal<HashSet<int>> mixer = new(()=>new(RouteItems.Length));

    public static Route CreateRandom(int length, int[][] costMatrix, Random random)
    {
        var range = Enumerable.Range(0, length);
        var permutation = range.OrderBy(_ => random.NextDouble()).ToArray();
        return new Route(permutation, costMatrix);
    }

    public double Fitness(object target)
    {
        if (target is not RouteTarget routeTarget) throw new ArgumentException(nameof(target));

        double cost = 1 / Cost();

        return cost;
    }

    public double Cost() =>
        RouteItems.Skip(1)
            .Aggregate((cost: 0, prev: RouteItems[0]),
                (result, x) => (cost: result.cost + CostMatrix[result.prev][x], prev: x)).cost + CostMatrix[RouteItems[^1]][RouteItems[0]];

    public IChromosome<int> Crossover(IChromosome<int> other, Random random)
    {
        if (other is not Route route || route.RouteItems.Length != RouteItems.Length) throw new ArgumentException(nameof(other));

        //return new Route(RouteItems.Concat(route.RouteItems).Distinct().ToArray(), CostMatrix);
        (int indexA, int indexB) = PickTwo(random, RouteItems.Length);

        foreach (var i in RouteItems[indexA..indexB])
        {
            mixer.Value.Add(i);
        }

        int[] newItems = new int[RouteItems.Length];
        Array.Copy(RouteItems,indexA, newItems, indexA, indexB-indexA+1);
        int nextIndex = 0;

        foreach (var i in route.RouteItems)
        {
            while (nextIndex >= indexA && nextIndex < indexB) nextIndex++;
                
            if(!mixer.Value.Contains(i))
            {
                mixer.Value.Add(i);
                newItems[nextIndex++] = i;
            }

            if (mixer.Value.Count == RouteItems.Length || nextIndex >= RouteItems.Length) break;
        }

        mixer.Value.Clear();

        //Debug.Assert(newItems.Distinct().Count() == newItems.Length);

        return new Route(newItems, CostMatrix);
    }

    public void Mutate(double mutationRate, Random random)
    {
        if (random.NextDouble() < mutationRate) return;

        (int indexA, int indexB) = PickTwo(random, RouteItems.Length);

        (RouteItems[indexA], RouteItems[indexB]) = (RouteItems[indexB], RouteItems[indexA]);
    }

    private (int,int) PickTwo(Random random, int max)
    {
        int indexA = random.Next(0, max);
        int indexB = random.Next(0, indexA);
        return (indexB, indexA);
    }

    public override string ToString() => string.Join(',', RouteItems) + $" Cost: {Cost()}";

    public virtual bool Equals(Route? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return RouteItems.SequenceEqual(other.RouteItems);
    }

    public override int GetHashCode()
    {
        return RouteItems.Sum(x => x.GetHashCode());
    }
}