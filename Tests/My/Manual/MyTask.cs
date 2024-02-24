using System.Runtime.CompilerServices;

namespace Tests.My.Manual;

public class MyTask
{
    public MyAwaiter GetAwaiter()
    {
        return new MyAwaiter();
    }
}

public class MyAwaiter : INotifyCompletion
{
    public bool IsCompleted { get; private set; }
    
    public void OnCompleted(Action continuation)
    {
        IsCompleted = true;
        continuation?.Invoke();
    }

    public int GetResult()
    {
        return 42;
    }
}

public class MyTaskBasedOnTask
{
    public TaskAwaiter GetAwaiter()
    {
        return Task.Delay(100).GetAwaiter();
    }
}