namespace Tests.My.AsyncVoid;

[TestClass]
public class AsyncVoidTests
{
    [TestMethod]
    public void потеря_исключения_в_async_void()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);
        
        sut.Delete();
    }
    
    [TestMethod]
    public void потеря_исключения_в_async_void_исключение_возникает_в_другом_потоке()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);

        sut.DeleteWithTryCatch();
    }
    
    [TestMethod]
    public async Task ловим_исключение_в_async_Task()
    {
        var repoMock = new Repository();
        var sut = new SutService(repoMock);
        
        var func = () => sut.DeleteAsync();
        await Assert.ThrowsExceptionAsync<NotImplementedException>(func);
    }
}

public class Repository : IRepository
{
    public async void Delete()
    {
        Console.WriteLine($"Repo start: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(TimeSpan.FromSeconds(1));
        throw new NotImplementedException();
    }
    
    public async void DeleteWithTryCatch()
    {
        try
        {
            Console.WriteLine($"Repo start: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Repo exception: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(e);
        }
    }
    
    public async Task DeleteAsync()
    {
        Console.WriteLine($"Repo start: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(TimeSpan.FromSeconds(1));
        throw new NotImplementedException();
    }
}