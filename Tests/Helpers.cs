namespace Tests;

public static class Helpers
{
    public static void PrintThreadInfo(string label)
    {
        Console.WriteLine($"{label} ThreadId {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"{label} IsBackground: {Thread.CurrentThread.IsBackground}");
        Console.WriteLine($"{label} IsThreadPoolThread {Thread.CurrentThread.IsThreadPoolThread}");
        Console.WriteLine($"{label} ApartmentState {Thread.CurrentThread.GetApartmentState()}");
    }
}