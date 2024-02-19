namespace Tests._2.Timeout;

[TestClass]
public class TimeoutTests
{
    [TestMethod]
    public async Task retry_with_delay_then_exception()
    {
        var repoMock = new RepositoryMock();
        var sut = new Sut(repoMock);
        
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var func = sut.GetCount;
        await Assert.ThrowsExceptionAsync<NotImplementedException>(func);
        watch.Stop();
        
        Assert.IsTrue(watch.ElapsedMilliseconds > 3000);
        Assert.AreEqual(repoMock.GetCountCallTimes, 3);
    }
    
    [TestMethod]
    public async Task get_count_with_timeout_test_timeout_exception()
    {
        var repoMock = new RepositoryMock();
        var sut = new Sut(repoMock);
        
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var func = sut.GetCountWithTimeoutTest;
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
        watch.Stop();
        
        Assert.IsTrue(watch.ElapsedMilliseconds > 2000);
        Assert.IsTrue(watch.ElapsedMilliseconds < 3000);
        Assert.AreEqual(repoMock.GetCountWithTimeoutCallTimes, 1);
    }
    
    [TestMethod]
    public async Task get_count_with_timeout_timeout_exception()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var repoMock = new RepositoryMock();
        var sut = new Sut(repoMock);
        
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var func = () => sut.GetCountWithTimeout(cts.Token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
        watch.Stop();
        
        Assert.IsTrue(watch.ElapsedMilliseconds > 2000);
        Assert.IsTrue(watch.ElapsedMilliseconds < 3000);
        Assert.AreEqual(repoMock.GetCountWithTimeoutCallTimes, 1);
    }
}

public class RepositoryMock : IRepository
{
    public int GetCountCallTimes { get; private set; }
    public int GetCountWithTimeoutCallTimes { get; private set; }
    
    public Task<int> GetCount()
    {
        GetCountCallTimes++;
        throw new NotImplementedException();
    }
    
    public async Task<int> GetCountWithTimeout(CancellationToken cancellationToken)
    {
        GetCountWithTimeoutCallTimes++;
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        return 42;
    }
}