using Moq;

namespace Tests._2;

[TestClass]
public class Tests
{
    [TestMethod]
    public async Task retry_web_service()
    {
        var fakeWebService = new FakeWebService();
        var sut = new Sut(fakeWebService);
        
        var func = () => sut.DownloadStringWithRetries();
        
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
        Assert.AreEqual(4, fakeWebService.GetStringAsyncCallTimes);
    }
    
    [TestMethod]
    public async Task реализация_асинхронного_интерфейса_синхронно()
    {
        var syncWebService = new SyncWebService();

        Task<string> task = null;
        var func = () => task = syncWebService.GetStringAsync();
        
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
        Assert.AreEqual(true, task.IsFaulted);
        Assert.AreEqual(TaskStatus.Faulted, task.Status);
    }
}

public class SyncWebService : IWebService
{
    public async Task<string> GetStringAsync()
    {
        try
        {
            throw new TimeoutException();
        }
        catch (Exception e)
        {
            return await Task.FromException<string>(e);
        }
    }
}

public class FakeWebService : IWebService
{
    public int GetStringAsyncCallTimes { get; set; }
    public async Task<string> GetStringAsync()
    {
        GetStringAsyncCallTimes++;
        await Task.Delay(100);
        throw new TimeoutException();
    }
}