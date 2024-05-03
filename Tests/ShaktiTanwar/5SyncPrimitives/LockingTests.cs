using System.Diagnostics;

namespace Tests.ShaktiTanwar._5SyncPrimitives;

[TestClass]
public class LockingTests
{
    [TestMethod]
    public async Task write_file()
    {
        var range = Enumerable.Range(1, 1000);
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < range.Count(); i++) {
            Thread.Sleep(10);
            File.AppendAllText("test.txt", i.ToString());
        }
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task write_file_parallel_error()
    {
        var range = Enumerable.Range(1, 1000);
        var func = () => range.AsParallel().AsOrdered().ForAll(i => {
            Thread.Sleep(10);
            File.AppendAllText("test.txt", i.ToString());
        });
        Assert.ThrowsException<AggregateException>(func);
    }
    
    static object _locker = new object();
    
    [TestMethod]
    public async Task write_file_parallel_with_dumb_lock()
    {
        var range = Enumerable.Range(1, 1000);
        var watch = Stopwatch.StartNew();
        range.AsParallel().AsOrdered().ForAll(i => {
            lock (_locker)
            {
                Thread.Sleep(10);
                File.AppendAllText("test.txt", i.ToString());
            }
        });
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task write_file_parallel_with_lock()
    {
        var range = Enumerable.Range(1, 1000);
        var watch = Stopwatch.StartNew();
        range.AsParallel().AsOrdered().ForAll(i => {
            Thread.Sleep(10);
            lock (_locker)
            {
                File.AppendAllText("test.txt", i.ToString());
            }
        });
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task write_file_parallel_with_monitor()
    {
        var range = Enumerable.Range(1, 1000);
        var watch = Stopwatch.StartNew();
        range.AsParallel().AsOrdered().ForAll(i => {
            Thread.Sleep(10);
            Monitor.Enter(_locker);
            try {
                File.WriteAllText("test.txt", i.ToString());
            } finally {
                Monitor.Exit(_locker);
            }
        });
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
    
    private static Mutex _mutex = new Mutex(false, "Mutex");
    
    [TestMethod]
    public async Task write_file_parallel_with_mutex()
    {
        var range = Enumerable.Range(1, 1000);
        var watch = Stopwatch.StartNew();
        range.AsParallel().AsOrdered().ForAll(i => {
            Thread.Sleep(10);
            _mutex.WaitOne();
            File.AppendAllText("test.txt", i.ToString());
            _mutex.ReleaseMutex();
        });
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
    
    [TestMethod]
    public async Task semaphore_example()
    {
        var range = Enumerable.Range(1, 100);
        var watch = Stopwatch.StartNew();
        var semaphore = new Semaphore(3, 3);
        range.AsParallel().AsOrdered().ForAll(i => {
            semaphore.WaitOne();
            Console.WriteLine($"Index {i} making service call using Task {Task.CurrentId}");
            Thread.Sleep(10);
            Console.WriteLine($"Index {i} releasing semaphore using Task {Task.CurrentId}");
            semaphore.Release();
        });
        watch.Stop();
        Console.WriteLine($"Total time to write file is {watch.ElapsedMilliseconds}");
    }
}