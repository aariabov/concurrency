using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

namespace Tests.Albahari._22ParallelProgramming;

[TestClass]
public class _4ConcurrentCollections
{
    [TestMethod]
    public async Task ConcurrentDictionary()
    {
        var d = new ConcurrentDictionary<int, int>();
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            d[i] = 123;
        }
        Console.WriteLine($"Time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task dictionary_with_lock()
    {
        var dict = new Dictionary<int, int>();
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < 1000000; i++)
        {
            lock (dict)
                dict[i] = 123;
        }
        Console.WriteLine($"Time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task Queue()
    {
        using (var q = new PCQueue(1))
        {
            Thread.Sleep(1000);
            q.EnqueueTask(() => Console.WriteLine($"Foo {DateTime.Now}"));
            Thread.Sleep(1000);
            q.EnqueueTask(() => Console.WriteLine($"Far {DateTime.Now}"));
        }
    }

    public class PCQueue : IDisposable
    {
        BlockingCollection<Action> _taskQ = new BlockingCollection<Action>();

        public PCQueue(int workerCount)
        {
            // Create and start a separate Task for each consumer:
            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        public void Dispose()
        {
            _taskQ.CompleteAdding();
        }

        public void EnqueueTask(Action action)
        {
            _taskQ.Add(action);
        }

        void Consume()
        {
            // блокируем, пока не появятся новые элементы
            Console.WriteLine($"Consume {DateTime.Now}");
            foreach (Action action in _taskQ.GetConsumingEnumerable())
                action();
        }
    }

    [TestMethod]
    public async Task queue_task()
    {
        using (var pcQ = new PCQueueTask(1))
        {
            Task task1 = pcQ.Enqueue (() => Console.WriteLine ($"Too {DateTime.Now}"));
            Thread.Sleep(1000);
            Task task2 = pcQ.Enqueue (() => Console.WriteLine ($"Easy {DateTime.Now}"));
    
            task1.ContinueWith (_ => Console.WriteLine($"Task 1 complete {DateTime.Now}"));
            task2.ContinueWith (_ => Console.WriteLine($"Task 2 complete {DateTime.Now}"));
        }  
    }
    
    public class PCQueueTask : IDisposable
    {
        BlockingCollection<Task> _taskQ = new BlockingCollection<Task>();
  
        public PCQueueTask(int workerCount)
        {
            // Create and start a separate Task for each consumer:
            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew (Consume);
        }
  
        public Task Enqueue (Action action, CancellationToken cancelToken = default (CancellationToken))
        {
            var task = new Task (action, cancelToken);
            _taskQ.Add(task);
            return task;
        }
  
        public Task<TResult> Enqueue<TResult> (Func<TResult> func, 
            CancellationToken cancelToken = default (CancellationToken))
        {
            var task = new Task<TResult> (func, cancelToken);
            _taskQ.Add(task);
            return task;
        }
  
        void Consume()
        {
            Console.WriteLine($"Consume {DateTime.Now}");
            foreach (var task in _taskQ.GetConsumingEnumerable())
            {
                try 
                {
                    if (!task.IsCanceled) task.RunSynchronously();
                } 
                catch (InvalidOperationException) { }  // Race condition
            }
        }
  
        public void Dispose() { _taskQ.CompleteAdding(); }
    }

    [TestMethod]
    public async Task channel_single_producer_multiple_consumers()
    {
        Channel<string> channel = Channel.CreateBounded<string> (new BoundedChannelOptions(1000)
            {
                SingleReader = false,
                SingleWriter = true,
            });
  
        var producer = Produce().ContinueWith (_ => channel.Writer.Complete());
        var consumer1 = Consume(1);
        var consumer2 = Consume(2);

        await Task.WhenAll(consumer1, consumer2);

        async Task Produce()
        {
            for (int i = 0; i < 10; i++)
            {
                await channel.Writer.WriteAsync ($"Msg {i} {DateTime.Now}");
                await Task.Delay(1000);
            }
            Console.WriteLine ($"Producer done. {DateTime.Now}");
        }

        async Task Consume(int id)
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                if (channel.Reader.TryRead (out string data))
                {
                    Console.WriteLine($"Consumer {id}: {data} {DateTime.Now}");
                    await Task.Delay(2000);
                }
            }
            Console.WriteLine ($"Consumer {id} done {DateTime.Now}");
        }  
    }

    [TestMethod]
    public async Task channel_single_producer_single_consumers()
    {
        Channel<string> channel = Channel.CreateBounded<string> (new BoundedChannelOptions (1000)
            {
                SingleReader = true,
                SingleWriter = true,
            });
        var producer = Produce().ContinueWith (_ => channel.Writer.Complete());
        var consumer = Consume();
        await consumer;

        async Task Produce()
        {
            for (int i = 0; i < 10; i++)
            {
                await channel.Writer.WriteAsync ($"Msg {i} {DateTime.Now}");
                await Task.Delay(1000);
            }
            Console.WriteLine($"Producer done {DateTime.Now}");
        }

        async Task Consume()
        {
            while (await channel.Reader.WaitToReadAsync())
            {
                if (channel.Reader.TryRead(out string data))
                {
                    Console.WriteLine($"Consumer: {data} {DateTime.Now}");
                    await Task.Delay(2000);
                }
            }
            Console.WriteLine($"Consumer done {DateTime.Now}");
        }
    }

    [TestMethod]
    public async Task spin_lock()
    {
        var spinLock = new SpinLock (true);   // Enable owner tracking
        bool lockTaken = false;
        try
        {
            spinLock.Enter (ref lockTaken);
            Console.WriteLine($"Enter SpinLock {DateTime.Now}");
            Thread.Sleep(1000);
        }
        finally
        {
            if (lockTaken)
            {
                spinLock.Exit();
                Console.WriteLine($"Exit SpinLock {DateTime.Now}");
            }
        }
    }

    [TestMethod]
    public async Task spin_wait()
    {
        bool _proceed = false;
        var task = Task.Factory.StartNew (Test);
        Thread.Sleep(1000);
        _proceed = true;
        task.Wait();
        
        void Test()
        {
            Console.WriteLine($"Start {DateTime.Now}");
            SpinWait.SpinUntil (() => { Thread.MemoryBarrier(); return _proceed; });
            Console.WriteLine($"Done {DateTime.Now}");
        }
    }

    [TestMethod]
    public async Task Spin_Once()
    {
        bool _proceed = false;
        var task = Task.Run (Test);
        Thread.Sleep(1000);
        _proceed = true;
        task.Wait();
        
        void Test()
        {
            Console.WriteLine($"Start {DateTime.Now}");
            var spinWait = new SpinWait();
            while (!_proceed) { Thread.MemoryBarrier(); spinWait.SpinOnce(); }
            Console.WriteLine($"Done {DateTime.Now}");
        }
    }

    [TestMethod]
    public async Task CompareExchange()
    {
        int x = 2;
        var task1 = Task.Factory.StartNew (() => MultiplyXBy (3));
        var task2 = Task.Factory.StartNew (() => MultiplyXBy (4));
        var task3 = Task.Factory.StartNew (() => MultiplyXBy (5));
  
        Task.WaitAll(task1, task2, task3);
        Console.WriteLine($"X = {x}, {DateTime.Now}");
        
        void MultiplyXBy (int factor)
        {
            var spinWait = new SpinWait();
            while (true)
            {
                int snapshot1 = x;
                Thread.MemoryBarrier();
                int calc = snapshot1 * factor;
                int snapshot2 = Interlocked.CompareExchange (ref x, calc, snapshot1);
                Console.WriteLine($"X = {x}, {DateTime.Now}");
                if (snapshot1 == snapshot2) return;
                spinWait.SpinOnce();
            }
        }
    }
}
