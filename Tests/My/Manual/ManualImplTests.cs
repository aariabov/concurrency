using System.Net;

namespace Tests.My.Manual;

[TestClass]
public class ManualImplTests
{
    [TestMethod]
    public void имитация_нажатия_кнопки_и_обновления_интерфейса_через_callback()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);
        
        sut.UpdateCount_BtnClick_Callback();
        Thread.Sleep(1100);
    }
    
    [TestMethod]
    public void имитация_нажатия_кнопки_и_обновления_интерфейса_через_event()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);
        
        sut.UpdateCount_BtnClick_Event();
        Thread.Sleep(1100);
    }
    
    [TestMethod]
    public void имитация_нажатия_кнопки_и_обновления_интерфейса_через_async_result()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);
        
        sut.UpdateCount_BtnClick_AsyncResult();
        Thread.Sleep(1100);
    }
}

public delegate void GetCountEventHandler(object sender, GetCountEventHandlerArgs args);

public class GetCountEventHandlerArgs
{
    public int Count { get; set; }
    public GetCountEventHandlerArgs(int count)
    {
        Count = count;
    }
}

public class Repository : IRepository
{
    public event GetCountEventHandler? GetCountCompleted;
    
    public void GetCountOnEvent()
    {
        new Thread(() =>
        {
            Console.WriteLine($"Second start: {Thread.CurrentThread.ManagedThreadId}");
            Thread.CurrentThread.IsBackground = true; 
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Second end: {Thread.CurrentThread.ManagedThreadId}");
            GetCountCompleted?.Invoke(this, new GetCountEventHandlerArgs(42));
        }).Start();
        Console.WriteLine($"Repo end: {Thread.CurrentThread.ManagedThreadId}");
    }
    
    public void BeginGetCountAsyncCallback(AsyncCallback? callback, object unrelatedObject)
    {
        new Thread(() =>
        {
            Console.WriteLine($"Second start: {Thread.CurrentThread.ManagedThreadId}");
            Thread.CurrentThread.IsBackground = true; 
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Second end: {Thread.CurrentThread.ManagedThreadId}");
            callback?.Invoke(new AsyncResult(unrelatedObject));
        }).Start();
        Console.WriteLine($"Repo end: {Thread.CurrentThread.ManagedThreadId}");
    }

    public int EndGetCountAsyncCallback(IAsyncResult result)
    {
        return 42;
    }
    
    public void GetCount(Action<int> callback)
    {
        new Thread(() => 
        {
            Console.WriteLine($"Second start: {Thread.CurrentThread.ManagedThreadId}");
            Thread.CurrentThread.IsBackground = true; 
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Second end: {Thread.CurrentThread.ManagedThreadId}");
            callback(42);
        }).Start();
        Console.WriteLine($"Repo end: {Thread.CurrentThread.ManagedThreadId}");
    }
}

public class AsyncResult : IAsyncResult
{
    public AsyncResult(object? asyncState)
    {
        AsyncState = asyncState;
    }

    public object? AsyncState { get; }
    public WaitHandle AsyncWaitHandle { get; }
    public bool CompletedSynchronously { get; }
    public bool IsCompleted { get; }
}