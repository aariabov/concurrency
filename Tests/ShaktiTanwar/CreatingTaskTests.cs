namespace Tests.ShaktiTanwar;

[TestClass]
public class CreatingTaskTests
{
    [TestMethod]
    public async Task create_task_example()
    {
        new Task(() => Console.WriteLine($"new Task started")).Start();
        Task.Factory.StartNew(() => Console.WriteLine($"Task.Factory.StartNew started")).Wait();
        Task.Run(() => Console.WriteLine($"Task.Run started")).Wait();
        var delayTask = Task.Delay(10);
        delayTask.Wait();

        var resultTask = Task.FromResult(42);
        Console.WriteLine($"Task.FromResult: {resultTask.Result}");

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