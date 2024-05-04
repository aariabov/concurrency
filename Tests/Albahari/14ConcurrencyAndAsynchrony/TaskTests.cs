namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class TaskTests
{
    [TestMethod]
    public async Task starting_task()
    {
        Task.Run(() => Console.WriteLine("Foo"));
    }

    [TestMethod]
    public async Task task_wait()
    {
        Task task = Task.Run(() =>
        {
            Console.WriteLine("Task started");
            Thread.Sleep(2000);
            Console.WriteLine("Foo");
        });
        Console.WriteLine(task.IsCompleted); // False
        task.Wait(); // Blocks until task is complete
    }

    [TestMethod]
    public async Task long_running_task()
    {
        Task task = Task.Factory.StartNew(
            () =>
            {
                Console.WriteLine("Task started");
                Thread.Sleep(2000);
                Console.WriteLine("Foo");
            },
            TaskCreationOptions.LongRunning
        );

        task.Wait(); // Blocks until task is complete
    }

    [TestMethod]
    public async Task return_value()
    {
        Task<int> task = Task.Run(() =>
        {
            Console.WriteLine("Foo");
            return 3;
        });

        int result = task.Result; // Blocks if not already finished
        Console.WriteLine(result); // 3
    }

    [TestMethod]
    public async Task count_prime_numbers()
    {
        Task<int> primeNumberTask = Task.Run(
            () =>
                Enumerable
                    .Range(2, 3000000)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
        );

        Console.WriteLine("Task running...");
        Console.WriteLine("The answer is " + primeNumberTask.Result);
    }

    [TestMethod]
    public async Task exceptions()
    {
        // Start a Task that throws a NullReferenceException:
        Task task = Task.Run(() =>
        {
            throw null;
        });
        try
        {
            task.Wait();
        }
        catch (AggregateException aex)
        {
            if (aex.InnerException is NullReferenceException)
                Console.WriteLine("Null!");
            else
                throw;
        }
    }

    [TestMethod]
    public async Task continuations_get_awaiter()
    {
        Task<int> primeNumberTask = Task.Run(
            () =>
                Enumerable
                    .Range(2, 3000000)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
        );

        var awaiter = primeNumberTask.GetAwaiter();
        awaiter.OnCompleted(() =>
        {
            int result = awaiter.GetResult();
            Console.WriteLine(result); // Writes result
        });
        primeNumberTask.Wait();
    }

    [TestMethod]
    public async Task continuations_continue_with()
    {
        Task<int> primeNumberTask = Task.Run(
            () =>
                Enumerable
                    .Range(2, 3000000)
                    .Count(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1).All(i => n % i > 0))
        );

        var task = primeNumberTask.ContinueWith(antecedent =>
        {
            int result = antecedent.Result;
            Console.WriteLine(result);
        });
        task.Wait();
    }

    [TestMethod]
    public async Task task_completion_source()
    {
        var tcs = new TaskCompletionSource<int>();

        new Thread(() =>
        {
            Thread.Sleep(100);
            tcs.SetResult(42);
        }).Start();

        Task<int> task = tcs.Task; // Our "slave" task.
        Console.WriteLine(task.Result); // 42
    }

    [TestMethod]
    public async Task task_completion_source_custom_run()
    {
        Task<int> task = Run(() =>
        {
            Thread.Sleep(100);
            return 42;
        });
        Console.WriteLine(task.Result);

        Task<TResult> Run<TResult>(Func<TResult> function)
        {
            var tcs = new TaskCompletionSource<TResult>();
            new Thread(() =>
            {
                try
                {
                    tcs.SetResult(function());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }).Start();
            return tcs.Task;
        }
    }

    [TestMethod]
    public async Task task_completion_source_example()
    {
        var task = GetAnswerToLife();
        var awaiter = task.GetAwaiter();
        awaiter.OnCompleted(() => Console.WriteLine(awaiter.GetResult()));
        task.Wait();

        Task<int> GetAnswerToLife()
        {
            var tcs = new TaskCompletionSource<int>();
            var timer = new System.Timers.Timer(100) { AutoReset = false };
            timer.Elapsed += delegate
            {
                timer.Dispose();
                tcs.SetResult(42);
            };
            timer.Start();
            return tcs.Task;
        }
    }

    [TestMethod]
    public async Task delay_custom()
    {
        var task = Delay(100);
        task.GetAwaiter().OnCompleted(() => Console.WriteLine(42));
        task.Wait();

        Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            var timer = new System.Timers.Timer(milliseconds) { AutoReset = false };
            timer.Elapsed += delegate
            {
                timer.Dispose();
                tcs.SetResult(null);
            };
            timer.Start();
            return tcs.Task;
        }
    }

    [TestMethod]
    public async Task delay_custom_example()
    {
        for (int i = 0; i < 5; i++)
            Delay(1000).GetAwaiter().OnCompleted(() => Console.WriteLine(42));

        Thread.Sleep(2000);

        Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            var timer = new System.Timers.Timer(milliseconds) { AutoReset = false };
            timer.Elapsed += delegate
            {
                timer.Dispose();
                tcs.SetResult(null);
            };
            timer.Start();
            return tcs.Task;
        }
    }

    [TestMethod]
    public async Task task_delay()
    {
        Task.Delay(10).GetAwaiter().OnCompleted(() => Console.WriteLine(42));

        // Another way to attach a continuation:
        Task.Delay(10).ContinueWith(ant => Console.WriteLine(42));
        Thread.Sleep(20);
    }

    [TestMethod]
    public async Task value_task()
    {
        var vt1 = AnswerQuestionAsync("What's the answer to life?");
        var vt2 = AnswerQuestionAsync("Is the sun shining?");

        Console.WriteLine($"vt1.IsCompleted: {vt1.IsCompleted}"); // True
        Console.WriteLine($"vt2.IsCompleted: {vt2.IsCompleted}"); // False

        var a1 = await vt1;
        Console.WriteLine($"a1: {a1}"); // Immediate

        var a2 = await vt2;
        Console.WriteLine($"a2: {a2}"); // Takes 5 seconds to appear

        async ValueTask<string> AnswerQuestionAsync(string question)
        {
            if (question == "What's the answer to life?")
                return "42"; // ValueTask<string>

            return await AskCortanaAsync(question); // ValueTask<Task<string>>
        }

        async Task<string> AskCortanaAsync(string question)
        {
            await Task.Delay(100);
            return "I don't know.";
        }
    }
}
