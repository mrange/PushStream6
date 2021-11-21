using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PushStream6.Benchmark;

// Run the benchmarks
BenchmarkRunner.Run<PgoBenchmarks>();


[Config(typeof(BenchmarkConfig))]
public class PgoBenchmarks
{
    //
    // Benchmark 1: Devirtualize unknown virtual calls:
    //

    public IEnumerable<object> TestData()
    {
        // Test data for 'GuardedDevirtualization(ICollection<int>)'
        yield return new List<int>();
    }

    [Benchmark]
    [ArgumentsSource(nameof(TestData))]
    public void GuardedDevirtualization(ICollection<int> collection)
    {
        // a chain of unknown virtual calls...
        collection.Clear();
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);
    }


    //
    // Benchmark 2: Allow inliner to be way more aggressive than usual
    //              for profiled call-sites:
    //

    [Benchmark]
    public StringBuilder ProfileDrivingInlining()
    {
        StringBuilder sb = new();
        for (int i = 0; i < 1000; i++)
            sb.Append("hi"); // see https://twitter.com/EgorBo/status/1451149444183990273
        return sb;
    }


    //
    // Benchmark 3: Reorder hot-cold blocks for better performance
    //

    [Benchmark]
    [Arguments(42)]
    public string HotColdBlockReordering(int a)
    {
        if (a == 1)
            return "a is 1";
        if (a == 2)
            return "a is 2";
        if (a == 3)
            return "a is 3";
        if (a == 4)
            return "a is 4";
        if (a == 5)
            return "a is 5";
        return "a is too big"; // this branch is always taken in this benchmark (a is 42)
    }
}
