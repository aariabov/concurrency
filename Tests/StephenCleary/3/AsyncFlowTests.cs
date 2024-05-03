using System.Runtime.CompilerServices;

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

    [TestMethod]
    public async Task slow_range()
    {
        IAsyncEnumerable<int> values = SlowRange().WhereAwait(
            async value =>
            {
                await Task.Delay(10);
                return value % 2 == 0;
            });
        
        await foreach (int result in values)
        {
            Console.WriteLine(result);
        }
    }

    [TestMethod]
    public async Task slow_range_count_async()
    {
        int count = await SlowRange().CountAsync(value => value % 2 == 0);
        Console.WriteLine(count);
    }

    [TestMethod]
    public async Task slow_range_count_await_async()
    {
        int count = await SlowRange().CountAwaitAsync(
            async value =>
            {
                await Task.Delay(10);
                return value % 2 == 0;
            });
        Console.WriteLine(count);
    }

    [TestMethod]
    public async Task slow_range_cancel()
    {
        using var cts = new CancellationTokenSource(500);
        CancellationToken token = cts.Token;
        
        var func = async () =>
        {
            await foreach (int result in SlowRange(token))
            {
                Console.WriteLine(result);
            }
        };
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
    }

    [TestMethod]
    public async Task slow_range_cancel1()
    {
        var func = async () => await ConsumeSequence(SlowRange());
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
        
        async Task ConsumeSequence(IAsyncEnumerable<int> items)
        {
            using var cts = new CancellationTokenSource(500);
            CancellationToken token = cts.Token;
            await foreach (int result in items.WithCancellation(token))
            {
                Console.WriteLine(result);
            }
        }
    }

    private static async IAsyncEnumerable<int> SlowRange([EnumeratorCancellation] CancellationToken token = default)
    {
        for (int i = 0; i != 10; ++i)
        {
            await Task.Delay(i * 100, token);
            yield return i;
        }
    }
}