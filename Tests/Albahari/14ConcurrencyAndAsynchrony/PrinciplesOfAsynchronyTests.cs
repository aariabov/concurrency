namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class PrinciplesOfAsynchronyTests
{
    [TestMethod]
    public async Task display_prime_counts_sync()
    {
        DisplayPrimeCounts();
        void DisplayPrimeCounts()
        {
            for (int i = 0; i < 10; i++)
                Console.WriteLine(
                    GetPrimesCount(i * 1000000 + 2, 1000000)
                    + " primes between "
                    + (i * 1000000)
                    + " and "
                    + ((i + 1) * 1000000 - 1)
                );

            Console.WriteLine("Done!");
        }

        int GetPrimesCount(int start, int count)
        {
            return ParallelEnumerable
                .Range(start, count)
                .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0));
        }
    }

    [TestMethod]
    public async Task крупно_модульный_параллелизм() // все вычисления выделяем в одну большую задачу
    {
        var task = Task.Run(() => DisplayPrimeCounts());
        task.Wait();
        
        void DisplayPrimeCounts()
        {
            for (int i = 0; i < 10; i++)
                Console.WriteLine(
                    GetPrimesCount(i * 1000000 + 2, 1000000)
                    + " primes between "
                    + (i * 1000000)
                    + " and "
                    + ((i + 1) * 1000000 - 1)
                );

            Console.WriteLine("Done!");
        }

        int GetPrimesCount(int start, int count)
        {
            return ParallelEnumerable
                .Range(start, count)
                .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0));
        }
    }

    [TestMethod]
    public async Task мелко_модульный_параллелизм_GetPrimesCountAsync()
    {
        DisplayPrimeCounts();
        void DisplayPrimeCounts()
        {
            DisplayPrimeCountsFrom(0);
        }
        
        void DisplayPrimeCountsFrom(int i)
        {
            var task = GetPrimesCountAsync(i * 1000000 + 2, 1000000);
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                Console.WriteLine(
                    awaiter.GetResult()
                    + " primes between "
                    + (i * 1000000)
                    + " and "
                    + ((i + 1) * 1000000 - 1)
                );
                if (i++ < 10)
                    DisplayPrimeCountsFrom(i);
                else
                    Console.WriteLine("Done");
            });
            task.Wait();
        }

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
    public async Task мелко_модульный_параллелизм_DisplayPrimeCountsAsync()
    {
        var task = DisplayPrimeCountsAsync();
        Task DisplayPrimeCountsAsync()
        {
            var machine = new PrimesStateMachine();
            machine.DisplayPrimeCountsFrom(0);
            return machine.Task;
        }
        task.Wait();
    }

    class PrimesStateMachine // Even more awkward!!
    {
        TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        public Task Task
        {
            get { return _tcs.Task; }
        }

        public void DisplayPrimeCountsFrom(int i)
        {
            var awaiter = GetPrimesCountAsync(i * 1000000 + 2, 1000000).GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                Console.WriteLine(awaiter.GetResult());
                if (i++ < 10)
                    DisplayPrimeCountsFrom(i);
                else
                {
                    Console.WriteLine("Done");
                    _tcs.SetResult(null);
                }
            });
        }

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
    public async Task display_prime_counts_async()
    {
        var task = DisplayPrimeCountsAsync1();
        task.Wait();
    }

    async Task DisplayPrimeCountsAsync1()
    {
        for (int i = 0; i < 10; i++)
            Console.WriteLine(
                await GetPrimesCountAsync1(i * 1000000 + 2, 1000000)
                    + " primes between "
                    + (i * 1000000)
                    + " and "
                    + ((i + 1) * 1000000 - 1)
            );

        Console.WriteLine("Done!");
    }

    Task<int> GetPrimesCountAsync1(int start, int count)
    {
        return Task.Run(
            () =>
                ParallelEnumerable
                    .Range(start, count)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
        );
    }
}
