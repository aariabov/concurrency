namespace Tests.ShaktiTanwar;

[TestClass]
public class WaitTaskTests
{
    [TestMethod]
    public async Task task_wait()
    {
        var task = Task.Factory.StartNew(() =>
        {
            Console.WriteLine($"task started");
            Task.Delay(100).Wait();
            Console.WriteLine($"task completed");
        });

        var threadIdBefore = Environment.CurrentManagedThreadId;
        task.Wait();
        var threadIdAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine($"Main completed");
        
        Assert.AreEqual(threadIdBefore, threadIdAfter);
    }
    
    [TestMethod]
    public async Task task_wait_all()
    {
        var taskA = Task.Factory.StartNew(() => Console.WriteLine("TaskA finished"));
        var taskB = Task.Factory.StartNew(() => Console.WriteLine("TaskB finished"));
        var threadIdBefore = Environment.CurrentManagedThreadId;
        Task.WaitAll(taskA, taskB);
        var threadIdAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine("Calling method finishes");
        
        Assert.AreEqual(threadIdBefore, threadIdAfter);
    }
    
    [TestMethod]
    public async Task task_wait_any()
    {
        var taskA = Task.Factory.StartNew(() =>
        {
            Task.Delay(200).Wait();
            Console.WriteLine("TaskA finished");
        });
        var taskB = Task.Factory.StartNew(() =>
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskB finished");
        });
        var threadIdBefore = Environment.CurrentManagedThreadId;
        Task.WaitAny(taskA, taskB);
        var threadIdAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine("Calling method finishes");
        
        Assert.AreEqual(threadIdBefore, threadIdAfter);

        Assert.IsTrue(taskB.IsCompleted);
        Assert.AreEqual(taskB.Status, TaskStatus.RanToCompletion);
        
        Assert.IsFalse(taskA.IsCompleted);
        Assert.AreEqual(taskA.Status, TaskStatus.Running);
    }
    
    [TestMethod]
    public async Task task_when_all()
    {
        var taskA = Task.Factory.StartNew(() => 
        {
            Task.Delay(200).Wait();
            Console.WriteLine("TaskA finished");
        });
        var taskB = Task.Factory.StartNew(() => 
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskB finished");
        });
        var threadIdBefore = Environment.CurrentManagedThreadId;
        await Task.WhenAll(taskA, taskB);
        var threadIdAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine("Calling method finishes");
        
        Assert.AreNotEqual(threadIdBefore, threadIdAfter);
    }
    
    [TestMethod]
    public async Task task_when_any()
    {
        var taskA = Task.Factory.StartNew(() => 
        {
            Task.Delay(200).Wait();
            Console.WriteLine("TaskA finished");
        });
        var taskB = Task.Factory.StartNew(() => 
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskB finished");
        });
        var threadIdBefore = Environment.CurrentManagedThreadId;
        await Task.WhenAny(taskA, taskB);
        var threadIdAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine("Calling method finishes");
        
        Assert.AreNotEqual(threadIdBefore, threadIdAfter);
    }
}