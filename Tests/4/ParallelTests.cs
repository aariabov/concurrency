namespace Tests._4;

[TestClass]
public class ParallelTests
{
    [TestMethod]
    public void find_prime_numbers_example()
    {
        var sut = new Sut();
        var numbers = new [] { 1, 2, 3, 4 };

        var result = sut.FindPrimeNumbers(numbers);
        
        CollectionAssert.AreEqual(new long[]{ 2, 3, 5, 7 }, result);
    }
    
    [TestMethod]
    public void find_prime_numbers()
    {
        var sut = new Sut();
        var numbers = Enumerable.Range(1000, 1000).ToArray();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var result = sut.FindPrimeNumbers(numbers);
        watch.Stop();
        Console.WriteLine($"FindPrimeNumbers time: {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public void find_prime_numbers_parallel()
    {
        var sut = new Sut();
        var numbers = Enumerable.Range(1000, 1000).ToArray();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var result = sut.FindPrimeNumbersParallel(numbers);
        watch.Stop();
        Console.WriteLine($"FindPrimeNumbersParallel time: {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public void find_prime_numbers_parallel_with_break()
    {
        var sut = new Sut();
        var numbers = Enumerable.Range(1000, 1000).ToArray();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var result = sut.FindPrimeNumbersParallelWithBreak(numbers);
        watch.Stop();
        Console.WriteLine($"result count: {result.Length}");
        Console.WriteLine($"FindPrimeNumbersParallelWithBreak time: {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public void find_prime_numbers_parallel_with_cancel()
    {
        var sut = new Sut();
        var numbers = Enumerable.Range(1000, 1000).ToArray();
        var cts = new CancellationTokenSource(100);

        var func = () => sut.FindPrimeNumbersParallelWithCancel(numbers, cts.Token);
        Assert.ThrowsException<OperationCanceledException>(func);
    }
    
    [TestMethod]
    public void find_prime_numbers_parallel_with_lock()
    {
        var sut = new Sut();
        var numbers = Enumerable.Range(1000, 2000).ToArray();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var result = sut.FindPrimeNumbersParallelWithLock(numbers);
        watch.Stop();
        Console.WriteLine($"result count: {result.Length}");
        Console.WriteLine($"FindPrimeNumbersParallelWithLock time: {watch.ElapsedMilliseconds}");
    }
}