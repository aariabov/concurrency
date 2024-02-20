namespace Tests.My.Tasks;

public class SutService
{
    public int Method1()
    {
        Console.WriteLine($"{nameof(Method1)}: {Thread.CurrentThread.ManagedThreadId}");
        return Thread.CurrentThread.ManagedThreadId;
    }
    
    public int Method2()
    {
        Console.WriteLine($"{nameof(Method2)}: {Thread.CurrentThread.ManagedThreadId}");
        return Thread.CurrentThread.ManagedThreadId;
    }
}