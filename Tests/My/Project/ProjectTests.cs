namespace Tests.My.Project;

[TestClass]
public class ProjectTests
{
    [TestMethod]
    public async Task параллельные_запросы_данных_всех_страниц()
    {
        var perPage = 100;
        var totalItems = 3609;
        var totalPages = 37;
        var arr = Enumerable.Range(1, perPage).ToArray();

        var tasks = new List<Task<int[]>>();
        foreach (var page in Enumerable.Range(1, totalPages - 1))
        {
            var task = GetData(page, perPage, totalItems);
            tasks.Add(task);
        }

        var items = await Task.WhenAll(tasks);
        var result = arr.Concat(items.SelectMany(i => i)).ToArray();
        
        Assert.AreEqual(totalItems, result.Length);
        Assert.AreEqual(totalItems, result.Last());
    }

    private async Task<int[]> GetData(int page, int perPage, int totalItems, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        var from = page * perPage + 1;
        var count = Math.Min(totalItems - page * perPage, perPage);
        return Enumerable.Range(from, count).ToArray();
    }
    
    [TestMethod]
    public async Task параллельные_запросы_данных_всех_страниц_ловим_все_исключения()
    {
        var totalPages = 37;
        var tasks = new List<Task<int[]>>();
        foreach (var _ in Enumerable.Range(1, totalPages - 1))
        {
            var task = Task.FromException<int[]>(new Exception());
            tasks.Add(task);
        }

        var whenAllTask = Task.WhenAll(tasks);
        try
        {
            var items = await whenAllTask;
        }
        catch (Exception e)
        {
            Assert.IsNotNull(whenAllTask.Exception);
            Assert.IsInstanceOfType(whenAllTask.Exception, typeof(AggregateException));
            Assert.AreEqual(totalPages - 1, whenAllTask.Exception.InnerExceptions.Count);
        }
    }
    
    [TestMethod]
    public async Task параллельные_запросы_данных_всех_страниц_отмена_всех_тасков_при_исключении()
    {
        var perPage = 100;
        var totalItems = 3609;
        var totalPages = 37;

        var cts = new CancellationTokenSource();
        var tasks = new List<Task<int[]>>();
        foreach (var page in Enumerable.Range(1, totalPages - 1))
        {
            var task = GetDataWithExceptionAndCancel(page, perPage, totalItems, cts);
            tasks.Add(task);
        }

        var whenAllTask = Task.WhenAll(tasks);
        try
        {
            var items = await whenAllTask;
        }
        catch (Exception e)
        {
            Assert.IsNotNull(whenAllTask.Exception);
            Assert.IsInstanceOfType(whenAllTask.Exception, typeof(AggregateException));
            Assert.AreEqual(1, whenAllTask.Exception.InnerExceptions.Count);
        }
    }

    private async Task<int[]> GetDataWithExceptionAndCancel(int page, int perPage, int totalItems, CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(page * 10, cts.Token);
            if (page == 5)
            {
                throw new Exception();
            }

            Console.WriteLine($"Page {page}");
            return await GetData(page, perPage, totalItems, cts.Token);
        }
        catch (Exception e)
        {
            cts.Cancel();
            throw;
        }
    }
    
    [TestMethod]
    public async Task параллельные_запросы_данных_всех_страниц_игнор_исключений()
    {
        var perPage = 100;
        var totalItems = 3609;
        var totalPages = 37;
        var arr = Enumerable.Range(1, perPage).ToArray();

        var tasks = new List<Task<int[]>>();
        foreach (var page in Enumerable.Range(1, totalPages - 1))
        {
            var task = GetDataWithSingleException(page, perPage, totalItems);
            tasks.Add(task);
        }

        var items = await Task.WhenAll(tasks);
        var result = arr.Concat(items.SelectMany(i => i)).ToArray();
        
        Assert.AreEqual(totalItems - perPage, result.Length);
        Assert.AreEqual(totalItems, result.Last());
    }

    private async Task<int[]> GetDataWithSingleException(int page, int perPage, int totalItems)
    {
        try
        {
            await Task.Delay(page * 10);
            if (page == 5)
            {
                throw new Exception("Незначительное исключение");
            }

            Console.WriteLine($"Page {page}");
            return await GetData(page, perPage, totalItems);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Page {page}: {e.Message}");
            return Array.Empty<int>();
        }
    }
}