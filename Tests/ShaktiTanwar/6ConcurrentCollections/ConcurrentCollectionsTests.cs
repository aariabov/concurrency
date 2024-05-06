using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tests.ShaktiTanwar._6ConcurrentCollections;

[TestClass]
public class ConcurrentCollectionsTests
{
    [TestMethod]
    public async Task queue_parallel_problem()
    {
        Queue<int> cq = new Queue<int>();

        for (int i = 0; i < 500; i++)
            cq.Enqueue(i);

        int sum = 0;

        Parallel.For(
            0,
            500,
            (i) =>
            {
                int localSum = 0;
                int localValue;
                while (cq.TryDequeue(out localValue))
                {
                    Thread.Sleep(10);
                    localSum += localValue;
                }
                Interlocked.Add(ref sum, localSum);
            }
        );

        Console.WriteLine(
            $"Calculated Sum is {sum} and actual sum should be {Enumerable.Range(0, 500).Sum()}"
        );
    }

    static object _locker = new object();

    [TestMethod]
    public async Task queue_parallel_problem_fix_with_lock()
    {
        Queue<int> cq = new Queue<int>();

        // Populate the queue.
        for (int i = 0; i < 500; i++)
            cq.Enqueue(i);

        int sum = 0;

        var watch = new Stopwatch();
        watch.Start();
        Parallel.For(
            0,
            500,
            (i) =>
            {
                int localSum = 0;
                int localValue;
                Monitor.Enter(_locker);
                while (cq.TryDequeue(out localValue))
                {
                    Thread.Sleep(10);
                    localSum += localValue;
                }
                Monitor.Exit(_locker);
                Interlocked.Add(ref sum, localSum);
            }
        );

        Console.WriteLine(
            $"Calculated Sum is {sum} and actual sum should be {Enumerable.Range(0, 500).Sum()}, time {watch.ElapsedMilliseconds}"
        );
    }

    [TestMethod]
    public async Task queue_parallel_problem_fix_with_concurrent_queue()
    {
        ConcurrentQueue<int> cq = new ConcurrentQueue<int>();

        for (int i = 0; i < 500; i++)
            cq.Enqueue(i);

        int sum = 0;
        var watch = new Stopwatch();
        watch.Start();
        Parallel.For(
            0,
            500,
            (i) =>
            {
                int localSum = 0;
                int localValue;
                while (cq.TryDequeue(out localValue))
                {
                    Thread.Sleep(10);
                    localSum += localValue;
                }
                Interlocked.Add(ref sum, localSum);
            }
        );

        Console.WriteLine(
            $"Calculated Sum is {sum} and actual sum should be {Enumerable.Range(0, 500).Sum()}, time {watch.ElapsedMilliseconds}"
        );
    }

    [TestMethod]
    public async Task concurrent_stack()
    {
        ConcurrentStack<int> concurrentStack = new ConcurrentStack<int>();

        for (int i = 0; i < 500; i++)
        {
            concurrentStack.Push(i);
        }
        concurrentStack.PushRange(new[] { 1, 2, 3, 4, 5 });

        int sum = 0;

        Parallel.For(
            0,
            500,
            (i) =>
            {
                int localSum = 0;
                int localValue;
                while (concurrentStack.TryPop(out localValue))
                {
                    Thread.Sleep(10);
                    localSum += localValue;
                }
                Interlocked.Add(ref sum, localSum);
            }
        );

        Console.WriteLine($"outerSum = {sum}, and actual sum should be 124765");
    }

    static ConcurrentBag<int> concurrentBag = new ConcurrentBag<int>();
    [TestMethod]
    public async Task concurrent_bag()
    {
        ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);

        Task producerAndConsumerTask = Task.Factory.StartNew(() =>
        {
            for (int i = 1; i <= 3; ++i)
            {
                concurrentBag.Add(i);
            }
            //Allow second thread to add items
            manualResetEvent.Wait();

            while (concurrentBag.IsEmpty == false)
            {
                int item;
                if (concurrentBag.TryTake(out item))
                {
                    Console.WriteLine($"Item is {item}");
                }
            }
        });

        Task producerTask = Task.Factory.StartNew(() =>
        {
            for (int i = 4; i <= 6; ++i)
            {
                concurrentBag.Add(i);
            }
            manualResetEvent.Set();
        });

        Task.WaitAll(producerAndConsumerTask, producerTask);
    }
    
    [TestMethod]
    public async Task blocking_collection()
    {
        BlockingCollection<int> blockingCollection = new BlockingCollection<int>(10);

        Task producerTask = Task.Factory.StartNew(() =>
        {
            for (int i = 0; i < 5; ++i)
            {
                blockingCollection.Add(i);
            }

            blockingCollection.CompleteAdding();
        });

        Task consumerTask = Task.Factory.StartNew(() =>
        {
            while (!blockingCollection.IsCompleted)
            {
                int item = blockingCollection.Take();
                Console.WriteLine($"Received item is {item}" );
            }
        });

        Task.WaitAll(producerTask, consumerTask);
    }
    
    [TestMethod]
    public async Task blocking_collection_multiple()
    {
        BlockingCollection<int>[] produceCollections = new BlockingCollection<int>[2];
        produceCollections[0] = new BlockingCollection<int>(5);
        produceCollections[1] = new BlockingCollection<int>(5);

        Task producerTask1 = Task.Factory.StartNew(() =>
        {
            for (int i = 1; i <= 5; ++i)
            {
                produceCollections[0].Add(i);
                Thread.Sleep(100);
            }
            produceCollections[0].CompleteAdding();
        });

        Task producerTask2 = Task.Factory.StartNew(() =>
        {
            for (int i = 6; i <= 10; ++i)
            {
                produceCollections[1].Add(i);
                Thread.Sleep(200);
            }
            produceCollections[1].CompleteAdding();
        });

        while (!produceCollections[0].IsCompleted || !produceCollections[1].IsCompleted)
        {
            int item;
            BlockingCollection<int>.TryTakeFromAny(produceCollections, out item, TimeSpan.FromSeconds(1));
            if (item != default(int))
            {
                Console.WriteLine($"Fetched item is {item}");
            }
        }
    }
    
    [TestMethod]
    public async Task concurrent_dictionary()
    {
        ConcurrentDictionary<int, string> concurrentDictionary = new ConcurrentDictionary<int, string>();

        Task producerTask1 = Task.Factory.StartNew(() => {
            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(100);
                concurrentDictionary.TryAdd(i, (i * i).ToString());
            }
        });
        Task producerTask2 = Task.Factory.StartNew(() => {
            for (int i = 10; i < 25; i++)
            {
                Thread.Sleep(100);
                concurrentDictionary.TryAdd(i, (i * i).ToString());
            }
        });
        Task producerTask3 = Task.Factory.StartNew(() => {
            for (int i = 15; i < 20; i++)
            {
                Thread.Sleep(100);
                concurrentDictionary.AddOrUpdate(i, (i * i).ToString(),(key, value) => (key * key).ToString());
            }
        });

        Task.WaitAll(producerTask1, producerTask2);

        Console.WriteLine($"Keys are {string.Join(", ", concurrentDictionary.Keys.Select(c => c.ToString()).ToArray())} " );
    }
}
