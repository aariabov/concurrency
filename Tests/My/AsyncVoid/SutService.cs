namespace Tests.My.AsyncVoid;

public interface IRepository
{
    void Delete();
    void DeleteWithTryCatch();
    Task DeleteAsync();
}

public class SutService
{
    private readonly IRepository _repository;

    public SutService(IRepository repository)
    {
        _repository = repository;
    }

    public void Delete()
    {
        try
        {
            Console.WriteLine($"Service start: {Thread.CurrentThread.ManagedThreadId}");
            // не ждем завершения метода, поэтому не ловим исключение - оно возникает в другом потоке
            _repository.Delete();
            Console.WriteLine($"Service end: {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Service exception: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(e);
        }
    }

    public void DeleteWithTryCatch()
    {
        try
        {
            Console.WriteLine($"Service start: {Thread.CurrentThread.ManagedThreadId}");
            // ждем пока сработает исключение и смотрим его поток
            _repository.DeleteWithTryCatch();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine($"Service end: {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Service exception: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DeleteAsync()
    {
        var task = _repository.DeleteAsync();
        Console.WriteLine($"Task info: Status - {task.Status}, IsCompleted - {task.IsCompleted}");
        try
        {
            Console.WriteLine($"Service start: {Thread.CurrentThread.ManagedThreadId}");
            await task;
            Console.WriteLine($"Service end: {Thread.CurrentThread.ManagedThreadId}");
        }
        catch (Exception e)
        {
            // исключение возникает в другом потоке
            Console.WriteLine($"Service exception: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Task info: Status - {task.Status}, IsCompleted - {task.IsCompleted}");
            Console.WriteLine(e);
            throw;
        }
    }
}