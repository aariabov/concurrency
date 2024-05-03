using System.Diagnostics;

namespace Tests.ShaktiTanwar._5SyncPrimitives;

[TestClass]
public class InterlockedTests
{
    static long _counter;
    
    [TestMethod]
    public async Task sync_for()
    {
        var watch = new Stopwatch();
        watch.Start();
        for (int i = 1; i < 1000; i++)
        {
            _counter++;
            Thread.Sleep(10);
        }
        Console.WriteLine($"Value for counter should be 999 and is {_counter}, time {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task race_condition()
    {
        var watch = new Stopwatch();
        watch.Start();
        Parallel.For(1, 1000, i =>
        {
            Thread.Sleep(10);
            _counter++;
        });
        Console.WriteLine($"Value for counter should be 999 and is {_counter}, time {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task interlocked_increment()
    {
        var watch = new Stopwatch();
        watch.Start();
        Parallel.For(1, 1000, i =>
        {
            Thread.Sleep(10);
            Interlocked.Increment(ref _counter);
        });
        Console.WriteLine($"Value for counter should be 999 and is {_counter}, time {watch.ElapsedMilliseconds}" );
    }
}