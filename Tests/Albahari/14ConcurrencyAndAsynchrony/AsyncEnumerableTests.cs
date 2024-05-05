using System.Diagnostics;

namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class AsyncEnumerableTests
{
    [TestMethod]
    public async Task enumerable_range()
    {
        Console.WriteLine("Starting async Task<IEnumerable<int>>. Data arrives in one group.");

        var watch = new Stopwatch();
        watch.Start();
        foreach (var number in await RangeTaskAsync(0, 10, 500)) // ждем 5сек, пока не получим все элементы
            Console.WriteLine($"{watch.ElapsedMilliseconds} {number}");

        static async Task<IEnumerable<int>> RangeTaskAsync(int start, int count, int delay)
        {
            List<int> data = new List<int>();
            for (int i = start; i < start + count; i++)
            {
                await Task.Delay(delay);
                data.Add(i);
            }

            return data;
        }
    }

    [TestMethod]
    public async Task async_enumerable_range()
    {
        Console.WriteLine($"Starting async Task<IEnumerable<int>>. Data arrives as available.");

        var watch = new Stopwatch();
        watch.Start();
        await foreach (var number in RangeAsync(0, 10, 500)) // сразу же выводим елемент, как он станет доступен
            Console.WriteLine($"{watch.ElapsedMilliseconds} {number}");

        async IAsyncEnumerable<int> RangeAsync(int start, int count, int delay)
        {
            for (int i = start; i < start + count; i++)
            {
                await Task.Delay(delay);
                yield return i;
            }
        }
    }

    [TestMethod]
    public async Task async_enumerable_linq()
    {
        IAsyncEnumerable<int> query =
            from i in RangeAsync(0, 10, 500)
            where i % 2 == 0
            select i * 10;

        var watch = new Stopwatch();
        watch.Start();
        await foreach (var number in query)
            Console.WriteLine($"{watch.ElapsedMilliseconds} {number}");

        async IAsyncEnumerable<int> RangeAsync(int start, int count, int delay)
        {
            for (int i = start; i < start + count; i++)
            {
                await Task.Delay(delay);
                yield return i;
            }
        }
    }
}
