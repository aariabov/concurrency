namespace Tests.My.Tasks.WhenAll;

public class SutService
{
    
    
    public async Task<int> WhenAllSum(int from, int to)
    {
        Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
        var tasks = new List<Task<int>>();
        foreach (var i in Enumerable.Range(from, to))
        {
            var task = Task.Run(() =>
            {
                Console.WriteLine($"Task{i}: {Thread.CurrentThread.ManagedThreadId}");
                return i;
            });
            tasks.Add(task);
        }

        var values = await Task.WhenAll(tasks);
        Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
        return values.Sum();
    }
    
    public async Task<int> WhenAllFirstException(int from, int to)
    {
        Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
        var tasks = new List<Task<int>>();
        foreach (var i in Enumerable.Range(from, to))
        {
            var task = Task.Run(() =>
            {
                Console.WriteLine($"Task{i}: {Thread.CurrentThread.ManagedThreadId}");
                throw new TimeoutException();
                return 1;
            });
            tasks.Add(task);
        }

        var values = await Task.WhenAll(tasks);
        Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
        return values.Sum();
    }
    
    public async Task<int> WhenAllCatchAllExceptions(int from, int to)
    {
        Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
        var tasks = new List<Task<int>>();
        foreach (var i in Enumerable.Range(from, to))
        {
            var task = Task.Run(() =>
            {
                Console.WriteLine($"Task{i}: {Thread.CurrentThread.ManagedThreadId}");
                throw new TimeoutException();
                return 1;
            });
            tasks.Add(task);
        }
        var whenAllTask = Task.WhenAll(tasks);

        try
        {
            var values = await whenAllTask;
            Console.WriteLine($"WhenAll: {Thread.CurrentThread.ManagedThreadId}");
            return values.Sum();
        }
        catch (Exception)
        {
            throw whenAllTask.Exception;
        }
    }
}