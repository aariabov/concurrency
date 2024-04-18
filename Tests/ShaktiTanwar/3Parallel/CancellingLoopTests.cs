namespace Tests.ShaktiTanwar._3Parallel;

[TestClass]
public class CancellingLoopTests
{
    [TestMethod]
    public async Task break_parallel_foreach()
    {
        var numbers = Enumerable.Range(1, 1000);
        int numToFind = 2;
        var result = Parallel.ForEach(numbers, (number, parallelLoopState) =>
        {
            Console.Write(number + " ");
            if (number == numToFind)
            {
                Console.WriteLine($"Calling Break at {number}");
                parallelLoopState.Break();
            }
        });
        Assert.IsFalse(result.IsCompleted);
        Assert.AreEqual(1, result.LowestBreakIteration);
    }
    
    [TestMethod]
    public async Task break_parallel_foreach_1()
    {
        var numbers = Enumerable.Range(1, 1000);
        var result = Parallel.ForEach(numbers, (i, parallelLoopState) => {
            Console.WriteLine($"For i={i} LowestBreakIteration = {parallelLoopState.LowestBreakIteration } and Task id = {Task.CurrentId}");
            if (i >= 10) 
            {
                parallelLoopState.Break();
            }
        });
        Assert.IsFalse(result.IsCompleted);
        Assert.AreEqual(9, result.LowestBreakIteration);
    }
    
    [TestMethod]
    public async Task stop_parallel_foreach()
    {
        var numbers = Enumerable.Range(1, 1000);
        var result = Parallel.ForEach(numbers, (i, parallelLoopState) =>
        {
            Console.Write(i + " ");
            if (i % 4 == 0)
            {
                Console.WriteLine("Loop Stopped on {0}", i);
                parallelLoopState.Stop();
            }
        });
        Assert.IsNull(result.LowestBreakIteration);
        Assert.IsFalse(result.IsCompleted);
    }
    
    [TestMethod]
    public async Task cancel_parallel_for()
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(100);

        ParallelOptions loopOptions = new ParallelOptions()
        {
            CancellationToken = cancellationTokenSource.Token
        };
        
        try
        {
            Parallel.For(0, 1000, loopOptions, index =>
            {
                Thread.Sleep(90);
                Console.WriteLine($"Index {index}");
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancellation exception caught!");
        }
    }
}