using System.Threading.Tasks.Dataflow;

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
    
    [TestMethod]
    // тестирование Dataflow
    public async Task test_block_example()
    {
        var myCustomBlock = CreateMyCustomBlock();
        
        myCustomBlock.Post(3);
        myCustomBlock.Post(13);
        myCustomBlock.Complete();
        
        Assert.AreEqual(4, myCustomBlock.Receive());
        Assert.AreEqual(14, myCustomBlock.Receive());
        await myCustomBlock.Completion;
    }
    
    IPropagatorBlock<int, int> CreateMyCustomBlock()
    {
        var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
        var addBlock = new TransformBlock<int, int>(item => item + 2);
        var divideBlock = new TransformBlock<int, int>(item => item / 2);
        var flowCompletion = new DataflowLinkOptions { PropagateCompletion = true };
        multiplyBlock.LinkTo(addBlock, flowCompletion);
        addBlock.LinkTo(divideBlock, flowCompletion);
        return DataflowBlock.Encapsulate(multiplyBlock, divideBlock);
    }
    
    [TestMethod]
    public async Task test_block_error_example()
    {
        var myCustomBlock = CreateMyCustomBlock();
        myCustomBlock.Post(3);
        myCustomBlock.Post(13);
        (myCustomBlock as IDataflowBlock).Fault(new InvalidOperationException());
        try
        {
            await myCustomBlock.Completion;
        }
        catch (AggregateException ex)
        {
            AssertExceptionIs<InvalidOperationException>(ex.Flatten().InnerException, false);
        }
    }
    
    public static void AssertExceptionIs<TException>(Exception ex, bool allowDerivedTypes = true)
    {
        if (allowDerivedTypes && !(ex is TException))
            Assert.Fail($"Exception is of type {ex.GetType().Name}, but " +
                        $"{typeof(TException).Name} or a derived type was expected.");
        if (!allowDerivedTypes && ex.GetType() != typeof(TException))
            Assert.Fail($"Exception is of type {ex.GetType().Name}, but " +
                        $"{typeof(TException).Name} was expected.");
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