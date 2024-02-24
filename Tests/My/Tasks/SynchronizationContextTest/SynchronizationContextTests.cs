using Nito.AsyncEx;

namespace Tests.My.Tasks.SynchronizationContextTest;

[TestClass]
public class SynchronizationContextTests
{
    [TestMethod]
    public async Task продолжить_в_том_же_потоке()
    {
        // используется библиотека для задания контекста
        AsyncContext.Run(async () =>
        {
            var sut = new SutService();
        
            await sut.ContinueWithSameThread();
        });
    }
    
    [TestMethod]
    public async Task synchronization_context_is_null()
    {
        Assert.IsNull(SynchronizationContext.Current);
    }
    
    [TestMethod]
    public async Task вызов_post_после_завершения_асинхронного_метода()
    {
        var mockContext = new MockSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(mockContext);
        
        await Task.Delay(100);
        
        Assert.AreEqual(1, mockContext.PostCallTimes);
    }
    
    [TestMethod]
    public async Task вызов_post_после_завершения_2х_асинхронных_методов()
    {
        var mockContext = new MockSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(mockContext);

        var sut = new SutService();
        await sut.TaskDelay();
        
        // 2 раза, тк есть продолжение в асинхронном методе и в самом тесте
        Assert.AreEqual(2, mockContext.PostCallTimes);
    }
}

public class MockSynchronizationContext : SynchronizationContext
{
    public int PostCallTimes { get; set; }

    public override void Post(SendOrPostCallback callback, object? state)
    {
        PostCallTimes++;
        callback(state);
    }
}