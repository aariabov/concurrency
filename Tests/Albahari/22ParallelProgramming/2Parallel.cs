using System.Diagnostics;
using System.Security.Cryptography;

namespace Tests.Albahari._22ParallelProgramming;

[TestClass]
public class _2Parallel
{
    [TestMethod]
    public async Task parallel()
    {
        var keyPairs = new string[6];

        Parallel.For(0, keyPairs.Length, i => keyPairs[i] = RSA.Create().ToXmlString(true));

        foreach (var keyPair in keyPairs)
        {
            Console.WriteLine(keyPair.Substring(0, 100));
        }
    }

    [TestMethod]
    public async Task plinq()
    {
        var keyPairs = ParallelEnumerable
            .Range(0, 6)
            .Select(i => RSA.Create().ToXmlString(true))
            .ToArray();

        foreach (var keyPair in keyPairs)
        {
            Console.WriteLine(keyPair.Substring(0, 100));
        }
    }

    [TestMethod]
    public async Task break_example()
    {
        Parallel.ForEach(
            "Hello, world",
            (c, loopState) =>
            {
                if (c == ',')
                    loopState.Stop();
                else
                    Console.Write(c);
            }
        );
    }

    [TestMethod]
    public async Task parallel_with_many_lock()
    {
        object locker = new object();
        double total = 0;
        var watch = Stopwatch.StartNew();
        // распараллелили, но из-за кучи блокировок теряем много времени
        Parallel.For(
            1,
            10000000,
            i =>
            {
                lock (locker)
                    total += Math.Sqrt(i);
            }
        );
        Console.WriteLine($"Total {total}, time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task parallel_with_few_lock()
    {
        object locker = new object();
        double total = 0;
        var watch = Stopwatch.StartNew();
        Parallel.For(
            1,
            10000000,
            () => 0.0,
            (i, state, localTotal) => // описываем локальное хранилище потока (внутри него не нужна синхронизация)
                localTotal + Math.Sqrt(i),
            localTotal => // объединяем локальное и глобальное хранилище, вот тут уже нужна синхронизация
            {
                lock (locker)
                    total += localTotal;
            }
        );
        Console.WriteLine($"Total {total}, time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task parallel_with_plinq()
    {
        var watch = Stopwatch.StartNew();
        double total = ParallelEnumerable.Range(1, 10000000)
            .Sum(i => Math.Sqrt(i));
        Console.WriteLine($"Total {total}, time {watch.ElapsedMilliseconds}");
    }
}
