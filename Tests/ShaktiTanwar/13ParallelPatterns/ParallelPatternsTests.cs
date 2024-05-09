using System.Collections.Concurrent;

namespace Tests.ShaktiTanwar._13ParallelPatterns;

[TestClass]
public class ParallelPatternsTests
{
    [TestMethod]
    public async Task async_enumerable_example()
    {
        IEnumerable<int> MapPositiveNumbers(int number)
        {
            IList<int> positiveNumbers = new List<int>();
            if (number > 0) positiveNumbers.Add(number);
            return positiveNumbers;
        }

        int GroupNumbers(int value) => value;
        IEnumerable<KeyValuePair<int, int>> ReduceNumbers(IGrouping<int, int> grouping) => new[] { new KeyValuePair<int, int>(grouping.Key, grouping.Count()) };

        IList<int> sourceData = new List<int>();
        var rand = new Random();
        for (int i = 0; i < 1000; i++)
        {
            sourceData.Add(rand.Next(-10, 10));
        }

        var result = sourceData
            .AsParallel()
            .MapReduce(MapPositiveNumbers, GroupNumbers, ReduceNumbers);
        
        foreach (var item in result)
        {
            Console.WriteLine($"{item.Key} found {item.Value} times");
        }
    }
    
    [TestMethod]
    public async Task aggregation_sync()
    {
        var output = new List<int>();
        var range = Enumerable.Range(1, 10);
        Func<int,int> action = (i) => i * i;
        range.ToList().ForEach(i =>
        {
            var result = action(i);
            output.Add(result);
        });

        output.ForEach(i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task aggregation_parallel_problem()
    {
        var output = new List<int>();
        var input = Enumerable.Range(1, 10);
        Func<int, int> action = (i) => i * i;

        Parallel.ForEach(input, item =>
        {
            var result = action(item);
            output.Add(result);
        });
        output.ForEach(i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task aggregation_parallel_with_lock()
    {
        var output = new List<int>();
        var input = Enumerable.Range(1, 10);
        Func<int, int> action = (i) => i * i;

        Parallel.ForEach(input, item =>
        {
            var result = action(item);
            lock (output) output.Add(result);
        });
        output.ForEach(i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task aggregation_with_concurrent_bag()
    {
        var input = Enumerable.Range(1, 10);
        Func<int, int> action = (i) => i * i;
        var output = new ConcurrentBag<int>();
        Parallel.ForEach(input, item =>
        {
            var result = action(item);
            output.Add(result);
        });
        output.ToList().ForEach(i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task aggregation_plinq()
    {
        var input = Enumerable.Range(1, 10);
        Func<int, int> action = (i) => i * i;
        var output = input.AsParallel()
            .Select(item => action(item))
            .ToList();
        output.ForEach(i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task lazy()
    {
        Lazy<Task<string>> lazy = new Lazy<Task<string>>(Task.Factory.StartNew(GetDataFromService));
        lazy.Value.ContinueWith((s)=> SaveToText(s.Result));
        lazy.Value.ContinueWith((s) => SaveToCsv(s.Result));
        
        string GetDataFromService()
        {
            Console.WriteLine("Service called");
            return "Some Dummy Data";
        }
        void SaveToText(string data)
        {
            Console.WriteLine("Save to Text called");
        }
        void SaveToCsv(string data)
        {
            Console.WriteLine("Save to CSV called");
        }
    }
}

public static class PatternExtensions
{
    public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
        this ParallelQuery<TSource> source,
        Func<TSource, IEnumerable<TMapped>> map,
        Func<TMapped, TKey> keySelector,
        Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce
    )
    {
        return source.SelectMany(map).GroupBy(keySelector).SelectMany(reduce);
    }
}
