namespace Tests.My.Tasks.SynchronizationContextTest;

public class SutService
{
    public async Task ContinueWithSameThread()
    {
        Console.WriteLine($"Init thread id: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(100);
        Console.WriteLine($"Continue thread id: {Thread.CurrentThread.ManagedThreadId}");
    }
    
    public async Task TaskDelay()
    {
        Console.WriteLine($"Start: {Thread.CurrentThread.ManagedThreadId}");
        // await Task.Delay(100).ContinueWith(task =>
        // {
        //     Console.WriteLine($"End: {Thread.CurrentThread.ManagedThreadId}");
        // });
        await Task.Delay(100);
        Console.WriteLine($"End: {Thread.CurrentThread.ManagedThreadId}");
    }
}