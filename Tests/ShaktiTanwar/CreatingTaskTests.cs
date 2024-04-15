namespace Tests.ShaktiTanwar;

[TestClass]
public class CreatingTaskTests
{
    [TestMethod]
    public async Task create_task_example()
    {
        new Task(() => Console.WriteLine($"new Task started")).Start();
        await Task.Factory.StartNew(() => Console.WriteLine($"Task.Factory.StartNew started"));
        await Task.Run(() => Console.WriteLine($"Task.Run started"));
        var delayTask = Task.Delay(10);
        await delayTask;

        var resultTask = Task.FromResult(42);
        Console.WriteLine($"Task.FromResult: {await resultTask}");

        var func = () => Task.FromException(new Exception("fake"));
        await Assert.ThrowsExceptionAsync<Exception>(func);
    }
    
    [TestMethod]
    public async Task task_yield_example()
    {
        for (int i = 0; i < 10000; i++)
        {
            if (i % 1000 == 0)
            {
                Helpers.PrintThreadInfo("Before Task.Yield()");
                await Task.Yield();
                Helpers.PrintThreadInfo("After Task.Yield()");
            }
        }
    }
}