namespace Tests.ShaktiTanwar;

[TestClass]
public class WaitingOnTasksTests
{
    [TestMethod]
    public async Task when_any()
    {
        Task taskA = Task.Factory.StartNew(() =>
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskA finished");
        });
        Task taskB = Task.Factory.StartNew(() =>
        {
            Task.Delay(50).Wait();
            Console.WriteLine("TaskB finished");
        });
        Task.WhenAny(taskA, taskB).Wait();
        Console.WriteLine("Calling method finishes");
    }

    [TestMethod]
    public async Task when_all()
    {
        Task taskA = Task.Factory.StartNew(() =>
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskA finished");
        });
        Task taskB = Task.Factory.StartNew(() =>
        {
            Task.Delay(50).Wait();
            Console.WriteLine("TaskB finished");
        });
        Task.WhenAll(taskA, taskB).Wait();
        Console.WriteLine("Calling method finishes");
    }

    [TestMethod]
    public async Task wait_any()
    {
        Task taskA = Task.Factory.StartNew(() =>
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskA finished");
        });
        Task taskB = Task.Factory.StartNew(() =>
        {
            Task.Delay(50).Wait();
            Console.WriteLine("TaskB finished");
        });
        Task.WaitAny(taskA, taskB);
        Console.WriteLine("Calling method finishes");
    }

    [TestMethod]
    public async Task wait_all()
    {
        Task taskA = Task.Factory.StartNew(() =>
        {
            Task.Delay(10).Wait();
            Console.WriteLine("TaskA finished");
        });
        Task taskB = Task.Factory.StartNew(() =>
        {
            Task.Delay(50).Wait();
            Console.WriteLine("TaskB finished");
        });
        Task.WaitAll(taskA, taskB);
        Console.WriteLine("Calling method finishes");
    }
}