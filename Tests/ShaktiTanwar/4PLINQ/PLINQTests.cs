using System.Diagnostics;

namespace Tests.ShaktiTanwar._4PLINQ;

[TestClass]
public class PLINQTests
{
    [TestMethod]
    public async Task as_ordered()
    {
        var range = Enumerable.Range(1, 10);

        range.ToList().ForEach(i => Console.Write(i + "-"));
        Console.WriteLine("Sequential Ordered");

        var unordered = range.AsParallel().Select(i => i).ToList();
        unordered.ForEach(i => Console.Write(i + "-"));
        Console.WriteLine("UnOrdered");

        var ordered = range.AsParallel().AsOrdered().Select(i => i).ToList();
        ordered.ForEach(i => Console.Write(i + "-"));
        Console.WriteLine("Parallel Ordered");
    }
    
    [TestMethod]
    public async Task un_ordered()
    {
        var range = Enumerable.Range(1, 10000);
        
        var unOrdered = range.AsParallel()
            .AsOrdered().Take(10)
            .AsUnordered().Select(i => i * i).ToList();
        unOrdered.ForEach(i => Console.Write(i + "-"));
        Console.WriteLine("AsUnordered");
        
        var ordered = range.AsParallel().AsOrdered().Take(10).Select(i => i * i).ToList();
        ordered.ForEach(i => Console.Write(i + "-"));
        Console.WriteLine("AsOrdered");
    }
    
    [TestMethod]
    public async Task merge_options()
    {
        var range = ParallelEnumerable.Range(1, 100);
        ParallelQuery<int> notBufferedQuery = range
            //.WithMergeOptions(ParallelMergeOptions.FullyBuffered)
            // .WithMergeOptions(ParallelMergeOptions.NotBuffered)
            .WithMergeOptions(ParallelMergeOptions.AutoBuffered)
            .Where(i => i % 10 == 0)
            .Select(x => {
                Thread.Sleep(1000);
                return x;
            });
        var watch = Stopwatch.StartNew();
        
        foreach (var item in notBufferedQuery)
        {
            Console.WriteLine( $"{item}:{watch.ElapsedMilliseconds}");
        }
        
        Console.WriteLine($"\nNotBuffered Full Result returned in {watch.ElapsedMilliseconds} ms" );
        watch.Stop();
    }
    
    [TestMethod]
    public async Task exception_handling()
    {
        var range = ParallelEnumerable.Range(1, 20);
        ParallelQuery<int> query = range.Select(i => i / (i - 10)).WithDegreeOfParallelism(2);
        try
        {
            query.ForAll(i => Console.WriteLine(i));
        }
        catch (AggregateException aggregateException)
        {
            foreach (var ex in aggregateException.InnerExceptions)
            {
                Console.WriteLine(ex.Message);
                if (ex is DivideByZeroException)
                    Console.WriteLine("Attempt to divide by zero. Query stopped.");
            }
        }
    }
    
    [TestMethod]
    public async Task parallel_and_sequential()
    {
        var range = Enumerable.Range(1, 100);
        var result = range.AsParallel().Where(i => i % 2 == 0). // параллельно
            AsSequential().Where(i => i % 8 == 0). // последовательно
            AsParallel().OrderBy(i => i); // параллельно
        foreach (var i in result)
        {
            Console.WriteLine(i);
        }
    }
    
    [TestMethod]
    public async Task canceling()
    {
        var range = Enumerable.Range(1, int.MaxValue);
        CancellationTokenSource cs = new CancellationTokenSource(100);
            
        try
        {
            var result = range.AsParallel()
                .WithCancellation(cs.Token)
                .Select(number => number)
                .ToList();
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}