namespace Tests;

[TestClass]
public class AsyncMethodTests
{
    [TestMethod]
    // если фреймворк тестирования поддерживает тестирование асинхронных методов
    public async Task IsExist_ReturnsFalse()
    {
        var sut = new SutService();
        bool result = await sut.IsExist();
        Assert.IsFalse(result);
    }

    [TestMethod]
    // если фреймворк тестирования НЕ поддерживает тестирование асинхронных методов
    public void IsExist_ReturnsFalseSync()
    {
        var sut = new SutService();
        bool result = sut.IsExist().GetAwaiter().GetResult();
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    // мок зависимости, которая успешно выполняется
    public async Task GetCount_Success()
    {
        var expectedValue = 42;
        var sut = new SutService(new SynchronousSuccess(expectedValue));
        var result = await sut.GetCount();
        Assert.AreEqual(result, expectedValue);
    }
    
    [TestMethod]
    public async Task GetCount_кидает_исключение()
    {
        var sut = new SutService(new SynchronousError());
        var func = sut.GetCount;
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
    }
    
    [TestMethod]
    // мок зависимости, которая успешно выполняется
    public async Task GetCount_YieldSuccess()
    {
        var expectedValue = 42;
        var sut = new SutService(new AsynchronousSuccess(expectedValue));
        var result = await sut.GetCount();
        Assert.AreEqual(result, expectedValue);
    }
}

class AsynchronousSuccess : IRepository
{
    private readonly int _returnValue;

    public AsynchronousSuccess(int returnValue)
    {
        _returnValue = returnValue;
    }
    
    public async Task<int> GetCount()
    {
        await Task.Yield(); // Принудительно включить асинхронное поведение.
        return _returnValue;
    }
}

class SynchronousError : IRepository
{
    public Task<int> GetCount()
    {
        return Task.FromException<int>(new InvalidOperationException());
    }
}

class SynchronousSuccess : IRepository
{
    private readonly int _returnValue;

    public SynchronousSuccess(int returnValue)
    {
        _returnValue = returnValue;
    }

    public Task<int> GetCount()
    {
        return Task.FromResult(_returnValue);
    }
}