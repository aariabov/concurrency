namespace Tests._3;

[TestClass]
public class AsyncFlowTests
{
    [TestMethod]
    public async Task sync_enumerable()
    {
        var sut = new Sut();
        foreach (var value in sut.GetValues())
        {
            Console.WriteLine(value);
        }

        var res = sut.GetValues().GetEnumerator();

        res.MoveNext();
        Assert.AreEqual(10, res.Current);
        res.MoveNext();
        Assert.AreEqual(13, res.Current);
        CollectionAssert.AreEqual(new []{10, 13}, sut.GetValues().ToArray());
    }
    
    [TestMethod]
    public async Task async_enumerable()
    {
        var sut = new Sut();
        await foreach (var value in sut.GetValuesAsync())
        {
            Console.WriteLine(value);
        }

        var res = sut.GetValuesAsync().GetAsyncEnumerator();

        var valueTask1 = res.MoveNextAsync();
        await valueTask1;
        Assert.IsInstanceOfType(valueTask1, typeof(ValueTask<bool>));
        Assert.AreEqual(10, res.Current);
        
        var valueTask2 = res.MoveNextAsync();
        await valueTask2;
        Assert.IsInstanceOfType(valueTask2, typeof(ValueTask<bool>));
        Assert.AreEqual(13, res.Current);
    }
    
    [TestMethod]
    public async Task получения_данных_из_внешнего_хранилища()
    {
        var sut = new Sut();
        IAsyncEnumerable<string> data = sut.GetDataAsync();
        await foreach (var name in data)
        {
            Console.WriteLine(name);
        }
    }
}