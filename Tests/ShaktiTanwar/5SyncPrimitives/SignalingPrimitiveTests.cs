using System.Diagnostics;

namespace Tests.ShaktiTanwar._5SyncPrimitives;

[TestClass]
public class SignalingPrimitiveTests
{
    [TestMethod]
    public async Task child_thread_without_waiting()
    {
        int result = 0;
        Thread childThread = new Thread(() =>
        {
            Thread.Sleep(1000);
            result = 10;
        });
        childThread.Start();
        Assert.AreEqual(0, result);
    }
    
    [TestMethod]
    public async Task thread_join_wait()
    {
        int result = 0;
        Thread childThread = new Thread(() =>
        {
            Thread.Sleep(1000);
            result = 10;
        });
        childThread.Start();
        childThread.Join();
        Assert.AreEqual(10, result);
    }
    
    [TestMethod]
    public async Task auto_reset_event()
    {
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        Task signallingTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                autoResetEvent.Set();
            }
        });
        int sum = 0;
        var watch = new Stopwatch();
        watch.Start();
        Parallel.For(1, 10, (i) =>
        {
            Console.WriteLine($"Task with id {Task.CurrentId} waiting for signal to enter");
            autoResetEvent.WaitOne();
            Console.WriteLine($"Task with id {Task.CurrentId} received signal to enter, time {watch.ElapsedMilliseconds}");
            sum += i;
        });
        Assert.AreEqual(45, sum);
    }
    
    [TestMethod]
    public async Task manual_reset_event()
    {
        ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        var watch = new Stopwatch();
        watch.Start();
        Task signalOffTask = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Thread.Sleep(3000);
                Console.WriteLine($"Network is down, time {watch.ElapsedMilliseconds}");
                manualResetEvent.Reset(); // выключаем сеть
            }
        });
        Task signalOnTask = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Thread.Sleep(5000);
                Console.WriteLine($"Network is Up, time {watch.ElapsedMilliseconds}");
                manualResetEvent.Set(); // включаем сеть
            }
        });
        // создаем 3 группы по 5 параллельных задач, которые будут ожидать открытия сети
        for (int i = 0; i < 3; i++)
        {
            Parallel.For(0, 5, (j) =>
            {
                Console.WriteLine($"Task with id {Task.CurrentId} waiting for network to be up, time {watch.ElapsedMilliseconds}");
                manualResetEvent.WaitOne();
                Console.WriteLine($"Task with id {Task.CurrentId} making service call, time {watch.ElapsedMilliseconds}");
            });
            Thread.Sleep(3000);
        }
    }
    
    [TestMethod]
    public async Task wait_handle_wait_all()
    {
        List<WaitHandle> waitHandles = new List<WaitHandle>
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        ThreadPool.QueueUserWorkItem(FetchDataFromService1, waitHandles.First());
        ThreadPool.QueueUserWorkItem(FetchDataFromService2, waitHandles.Last());

        // ждем, пока все вызовы отработают (вызовут метод .Set())
        WaitHandle.WaitAll(waitHandles.ToArray());

        Assert.AreEqual(893, _dataFromService1 + _dataFromService2);
    }
    static int _dataFromService1 = 0;
    static int _dataFromService2 = 0;
    private static void FetchDataFromService1(object state)
    {
        Thread.Sleep(1000);
        _dataFromService1 = 890;
        var autoResetEvent = state as AutoResetEvent;
        autoResetEvent.Set();
    }
    private static void FetchDataFromService2(object state)
    {
        Thread.Sleep(1000);
        _dataFromService2 = 3;
        var autoResetEvent = state as AutoResetEvent;
        autoResetEvent.Set();
    }
    
    static int findIndex = -1;
    static string winnerAlgo = string.Empty;
    [TestMethod]
    public async Task wait_handle_wait_any()
    {
        WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };
        var itemToSearch = 15000;
        var range = Enumerable.Range(1, 100000).ToArray();
        ThreadPool.QueueUserWorkItem(LinearSearch, new { Range = range, ItemToFind = itemToSearch, WaitHandle = waitHandles[0] });
        ThreadPool.QueueUserWorkItem(BinarySearch, new { Range = range, ItemToFind = itemToSearch, WaitHandle = waitHandles[1] });

        // ждем самый быстрый алгоритм
        WaitHandle.WaitAny(waitHandles);

        Console.WriteLine($"Item found at index {findIndex} and faster algo is {winnerAlgo}");
    }

    private static void BinarySearch(object state)
    {
        dynamic data = state;
        int[] x = data.Range;
        int valueToFind = data.ItemToFind;
        AutoResetEvent autoResetEvent = data.WaitHandle as AutoResetEvent;

        int foundIndex = Array.BinarySearch(x, valueToFind);

        Interlocked.CompareExchange(ref findIndex, foundIndex, -1);
        Interlocked.CompareExchange(ref winnerAlgo, "BinarySearch", string.Empty);

        autoResetEvent.Set();
    }

    public static void LinearSearch(object state)
    {
        dynamic data = state;
        int[] x = data.Range;
        int valueToFind = data.ItemToFind;
        AutoResetEvent autoResetEvent = data.WaitHandle as AutoResetEvent;
        int foundIndex = -1;
        for (int i = 0; i < x.Length; i++)
        {
            if (valueToFind == x[i])
            {
                foundIndex = i;
            }
        }
        Interlocked.CompareExchange(ref findIndex, foundIndex, -1);
        Interlocked.CompareExchange(ref winnerAlgo, "LinearSearch", string.Empty);

        autoResetEvent.Set();
    }
}