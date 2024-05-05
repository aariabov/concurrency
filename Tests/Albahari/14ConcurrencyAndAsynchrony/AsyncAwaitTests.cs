namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class AsyncAwaitTests
{
    [TestMethod]
    public async Task async_await_example()
    {
        var res = await RunTask();
        Assert.AreEqual(res, 42);

        Task<int> RunTask()
        {
            return Task.Run(() => Task.FromResult(42));
        }
    }

    [TestMethod]
    public async Task without_async_await()
    {
        var awaiter = RunTask().GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var res = awaiter.GetResult();
            Assert.AreEqual(res, 42);
        });

        Task<int> RunTask()
        {
            return Task.Run(() => Task.FromResult(42));
        }
    }

    [TestMethod]
    public async Task async_await_prime()
    {
        int result = await GetPrimesCountAsync(2, 1000000);
        Assert.AreEqual(result, 78498);

        Task<int> GetPrimesCountAsync(int start, int count)
        {
            return Task.Run(
                () =>
                    ParallelEnumerable
                        .Range(start, count)
                        .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
            );
        }
    }

    [TestMethod]
    public async Task without_async_await_prime()
    {
        var awaiter = GetPrimesCountAsync(2, 1000000).GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            var result = awaiter.GetResult();
            Assert.AreEqual(result, 78498);
        });

        Task<int> GetPrimesCountAsync(int start, int count)
        {
            return Task.Run(
                () =>
                    ParallelEnumerable
                        .Range(start, count)
                        .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
            );
        }
    }

    [TestMethod]
    public async Task capturing_local_state()
    {
        for (int i = 0; i < 10; i++)
            Console.WriteLine ($"{i} {await GetPrimesCountAsync (i*1000000+2, 1000000)}");

        Task<int> GetPrimesCountAsync (int start, int count)
        {
            return Task.Run (() =>
                ParallelEnumerable.Range (start, count).Count (n => 
                    Enumerable.Range (2, (int)Math.Sqrt(n)-1).All (i => n % i > 0)));
        }
    }

    [TestMethod]
    public async Task parallel_tasks()
    {
        var task1 = PrintAnswerToLife();
        var task2 = PrintAnswerToLife();
        await task1; await task2;
        Console.WriteLine ("Done");
        
        async Task PrintAnswerToLife()
        {
            int answer = await GetAnswerToLife();
            Console.WriteLine (answer);
        }

        async Task<int> GetAnswerToLife()
        {
            await Task.Delay(100);
            int answer = 21 * 2;
            return answer;
        }
    }

    [TestMethod]
    public async Task completed_tasks()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"{i} square is {await Square(i)}, threadId {Environment.CurrentManagedThreadId}");
        }

        async Task<int> Square(int i)
        {
            if (i % 2 == 0)
            {
                return i * i;
            }
            else
            {
                await Task.Delay(10);
                return await Task.FromResult(i * i);
            }
        }
    }
}
