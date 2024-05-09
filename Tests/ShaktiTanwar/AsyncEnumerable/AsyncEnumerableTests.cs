using System.Diagnostics;

namespace Tests.ShaktiTanwar.AsyncEnumerable;

[TestClass]
public class AsyncEnumerableTests
{
    [TestMethod]
    public async Task async_enumerable_example()
    {
        await foreach (var i in Test())
        {
            Console.WriteLine(i);
        }

        async IAsyncEnumerable<int> Test()
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                yield return i;
            }
        }
    }

    [TestMethod]
    public async Task custom_async_enumerable()
    {
        var watch = Stopwatch.StartNew();
        await foreach(var dataPoint in GetBigResultsAsync()) {
            Console.WriteLine($"{dataPoint}, time {watch.ElapsedMilliseconds}");
        }
        
        async IAsyncEnumerable<int> GetBigResultsAsync() {
            var list = Enumerable.Range(1, 10);
            await foreach(var item in list.AsEnumerable()) 
            {
                yield return item;
            }
        } 
    }
}

public class OddIndexEnumerator : IAsyncEnumerator<int>
{
    List<int> _numbers;
    int _currentIndex = 1;

    public OddIndexEnumerator(IEnumerable<int> numbers)
    {
        _numbers = numbers.ToList();
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask<bool> MoveNextAsync()
    {
        Thread.Sleep(100);
        if (_currentIndex < _numbers.Count() - 2)
        {
            _currentIndex += 2;
            return new ValueTask<bool>(Task.FromResult<bool>(true));
        }
        return new ValueTask<bool>(Task.FromResult<bool>(false));
    }

    public int Current
    {
        get
        {
            Console.WriteLine($"Thread Id is {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(100);
            return _numbers[_currentIndex];
        }
    }
}

public class CustomAsyncIntegerCollection : IAsyncEnumerable<int>
{
    List<int> _numbers;

    public CustomAsyncIntegerCollection(IEnumerable<int> numbers)
    {
        _numbers = numbers.ToList();
    }

    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new OddIndexEnumerator(_numbers);
    }
}

public static class CollectionExtensions {
    public static IAsyncEnumerable<int> AsEnumerable(
        this IEnumerable<int> source) =>
        new CustomAsyncIntegerCollection(source);
}
