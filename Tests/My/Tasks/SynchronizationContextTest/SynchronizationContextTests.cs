using System.Diagnostics;
using Nito.AsyncEx;

namespace Tests.My.Tasks.SynchronizationContextTest;

[TestClass]
public class SynchronizationContextTests
{
    [TestMethod]
    public async Task продолжить_в_том_же_потоке()
    {
        // используется библиотека для задания контекста
        AsyncContext.Run(async () =>
        {
            var sut = new SutService();
        
            await sut.ContinueWithSameThread();
        });
    }
    
    [TestMethod]
    public async Task synchronization_context_is_null()
    {
        Assert.IsNull(SynchronizationContext.Current);
    }
    
    [TestMethod]
    public async Task вызов_post_после_завершения_асинхронного_метода()
    {
        var mockContext = new MockSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(mockContext);
        
        await Task.Delay(100);
        
        Assert.AreEqual(1, mockContext.PostCallTimes);
    }
    
    [TestMethod]
    public async Task вызов_post_после_завершения_2х_асинхронных_методов()
    {
        var mockContext = new MockSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(mockContext);

        var sut = new SutService();
        await sut.TaskDelay();
        
        // 2 раза, тк есть продолжение в асинхронном методе и в самом тесте
        Assert.AreEqual(2, mockContext.PostCallTimes);
    }
    
    [TestMethod]
    public async Task запуск_асинхронных_методов_без_родительского_контекста()
    {
        var mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine("Main: " + Thread.CurrentThread.ManagedThreadId);
        Assert.AreEqual(null, SynchronizationContext.Current);
        await Task.Run(() =>
        {
            Console.WriteLine($"Task run 1: {Thread.CurrentThread.ManagedThreadId}");
        });
        // продолжение выполняется в потоке из ThreadPool
        Assert.AreNotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine($"Продолжение1: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
        
        await Task.Run(() =>
        {
            Console.WriteLine("Task run 2: " + Thread.CurrentThread.ManagedThreadId);
        });
        Assert.AreNotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
        // продолжение выполняется в потоке из ThreadPool
        Console.WriteLine($"Продолжение2: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
    }
    
    [TestMethod]
    public async Task продолжение_выполняется_в_родительском_потоке()
    {
        AsyncContext.Run(async () =>
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Main: " + Thread.CurrentThread.ManagedThreadId);
            Assert.AreNotEqual(null, SynchronizationContext.Current);
            await Task.Run(() =>
            {
                Console.WriteLine($"Task run 1: {Thread.CurrentThread.ManagedThreadId}");
            });
            // продолжение выполняется в главном потоке
            Assert.AreEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"Продолжение1: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
        
            await Task.Run(() =>
            {
                Console.WriteLine("Task run 2: " + Thread.CurrentThread.ManagedThreadId);
            });
            // продолжение выполняется в главном потоке
            Assert.AreEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"Продолжение2: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
        });
    }
    
    [TestMethod]
    public async Task продолжение_выполняется_в_потоке_из_thread_pool()
    {
        AsyncContext.Run(async () =>
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("Main: " + Thread.CurrentThread.ManagedThreadId);
            Assert.AreNotEqual(null, SynchronizationContext.Current);
            await Task.Run(() =>
            {
                Console.WriteLine($"Task run 1: {Thread.CurrentThread.ManagedThreadId}");
            }).ConfigureAwait(false);
            // продолжение выполняется в потоке из ThreadPool
            Assert.AreNotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"Продолжение1: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
        
            await Task.Run(() =>
            {
                Console.WriteLine("Task run 2: " + Thread.CurrentThread.ManagedThreadId);
            }).ConfigureAwait(false);
            // продолжение выполняется в потоке из ThreadPool
            Assert.AreNotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"Продолжение2: {Thread.CurrentThread.ManagedThreadId}, {SynchronizationContext.Current}\n");
        });
    }
    
    [TestMethod]
    public async Task вызов_post_занимает_много_времени()
    {
        AsyncContext.Run(async () =>
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var mainContext = SynchronizationContext.Current;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var sum = await GetSum(() =>
            {
                Assert.AreEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
                Assert.AreEqual(SynchronizationContext.Current, mainContext);
            });
            stopwatch.Stop();
            Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}");
            
            Assert.AreEqual(1784293664, sum);
        });
    }
    
    [TestMethod]
    public async Task без_вызова_post_работает_быстрее()
    {
        var mainThreadId = Thread.CurrentThread.ManagedThreadId;
        Assert.IsNull(SynchronizationContext.Current);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var sum = await GetSum(() =>
        {
            Assert.AreNotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);
            Assert.IsNull(SynchronizationContext.Current);
        });
        stopwatch.Stop();
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}");
            
        Assert.AreEqual(1784293664, sum);
    }

    private static async Task<int> GetSum(Action action)
    {
        var sum = 0;
        foreach (var i in Enumerable.Range(1, 1000000))
        {
            var val = await Task.Run(() =>
            {
                Assert.IsNull(SynchronizationContext.Current);
                return i;
            });
            action();
            sum += val;
        }

        return sum;
    }
}

public class MockSynchronizationContext : SynchronizationContext
{
    public int PostCallTimes { get; set; }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        PostCallTimes++;
        callback(state);
    }
}