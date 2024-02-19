namespace Tests._2.ReturnCompletedTask;

[TestClass]
public class ReturnCompletedTaskTests
{
    [TestMethod]
    public async Task success()
    {
        var expectedCount = 42;
        var repoMock = new SuccessRepository(expectedCount);
        var sut = new Sut(repoMock);
        
        var count = await sut.GetCount();
        await sut.Delete();
        
        Assert.AreEqual(expectedCount, count);
    }
    
    [TestMethod]
    public async Task get_count_timeout_exception()
    {
        var repoMock = new TimeoutRepository();
        var sut = new Sut(repoMock);
        
        var func = () => sut.GetCount();
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
    }
    
    [TestMethod]
    public async Task canceled()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var repoMock = new CanceledRepository();
        var sut = new Sut(repoMock);
        
        cts.Cancel();
        var count = sut.GetCount(cts.Token);
        
        Assert.AreEqual(count.Status, TaskStatus.Canceled);
    }
    
    [TestMethod]
    public async Task canceled_exception()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var repoMock = new CanceledRepository();
        var sut = new Sut(repoMock);
        
        cts.Cancel();
        var func = () => sut.GetCount(cts.Token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
    }
}

class SuccessRepository : IRepository
{
    private readonly int _count;

    public SuccessRepository(int count)
    {
        _count = count;
    }

    public Task<int> GetCount(CancellationToken cancellationToken)
    {
        return Task.FromResult(_count);
    }
    
    public Task Delete(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

class TimeoutRepository : IRepository
{
    public Task<int> GetCount(CancellationToken cancellationToken)
    {
        return Task.FromException<int>(new TimeoutException());
    }

    public Task Delete(CancellationToken cancellationToken)
    {
        return Task.FromException(new TimeoutException());
    }
}

class CanceledRepository : IRepository
{
    public Task<int> GetCount(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<int>(cancellationToken);
        return Task.FromResult(13);
    }

    public Task Delete(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        return Task.CompletedTask;
    }
}