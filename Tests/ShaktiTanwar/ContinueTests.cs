namespace Tests.ShaktiTanwar;

[TestClass]
public class ContinueTests
{
    [TestMethod]
    public async Task continue_with()
    {
        var task = Task.FromResult(3);
        task.ContinueWith((e) => 
        {
            Console.WriteLine($"Result {e.Result * 2}" );
        }).Wait();
    }
    
    [TestMethod]
    public async Task continue_with_option()
    {
        var task = Task.FromResult(3);
        task.ContinueWith((e) => 
        {
            Console.WriteLine($"Result {e.Result * 2}" );
        }, TaskContinuationOptions.NotOnFaulted).Wait();
    }
    
    [TestMethod]
    public async Task continue_when_all()
    {
        int a = 2, b = 3;
        Task<int> taskA = Task.Factory.StartNew<int>(() => a * a);
        Task<int> taskB = Task.Factory.StartNew<int>(() => b * b);
        Task<int> taskC = Task.Factory.StartNew<int>(() => 2 * a * b);
        var sum = await Task.Factory.ContinueWhenAll<int>(new Task[] { taskA, taskB, taskC }
            , (tasks) => tasks.Sum(t => (t as Task<int>).Result));
        Console.WriteLine(sum);
    }
    
    [TestMethod]
    public async Task continue_when_any()
    {
        int number = 13;
        Task<bool> taskA = Task.Factory.StartNew<bool>(() => number / 2 != 0);
        Task<bool> taskB = Task.Factory.StartNew<bool>(() => (number / 2) * 2 != number);
        Task<bool> taskC = Task.Factory.StartNew<bool>(() => (number & 1) != 0);
        Task.Factory.ContinueWhenAny<bool>(new Task<bool>[] { taskA, taskB, taskC }, (task) =>
            {
                Console.WriteLine((task as Task<bool>).Result);
            }
        );  
    }
}