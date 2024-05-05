using System.Net;

namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class AsyncPatternTests
{
    [TestMethod]
    public async Task my_cancellation_token()
    {
        var token = new MyCancellationToken();
        Task.Delay(500).ContinueWith(ant => token.Cancel()); // Tell it to cancel in two seconds.
        var func = () => Foo(token);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(func);

        async Task Foo(MyCancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                await Task.Delay(100);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    class MyCancellationToken
    {
        public bool IsCancellationRequested { get; private set; }

        public void Cancel()
        {
            IsCancellationRequested = true;
        }

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
                throw new OperationCanceledException();
        }
    }

    [TestMethod]
    public async Task cancellation_token()
    {
        var cancelSource = new CancellationTokenSource();
        Task.Delay(500).ContinueWith(ant => cancelSource.Cancel()); // Tell it to cancel in two seconds.
        var func = () => Foo(cancelSource.Token);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(func);

        async Task Foo(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                await Task.Delay(100);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    [TestMethod]
    public async Task cancellation_token1()
    {
        var cancelSource = new CancellationTokenSource(500);
        var func = () => Foo(cancelSource.Token);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(func);

        async Task Foo(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                await Task.Delay(100);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    [TestMethod]
    public async Task custom_progress()
    {
        Action<int> progress = i => Console.WriteLine(i + " %");
        await Foo(progress);

        Task Foo(Action<int> onProgressPercentChanged)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (i % 10 == 0)
                        onProgressPercentChanged(i);
                }
            });
        }
    }

    [TestMethod]
    public async Task progress()
    {
        var progress = new Progress<int>(i => Console.WriteLine(i + " %"));
        await Foo(progress);

        Task Foo(IProgress<int> onProgressPercentChanged)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (i % 10 == 0)
                        onProgressPercentChanged.Report(i);
                }
            });
        }
    }

    [TestMethod]
    public async Task when_any()
    {
        int res = await await Task.WhenAny(Delay1(), Delay2(), Delay3());
        Assert.AreEqual(1, res);

        async Task<int> Delay1()
        {
            await Task.Delay(1000);
            return 1;
        }
        async Task<int> Delay2()
        {
            await Task.Delay(2000);
            return 2;
        }
        async Task<int> Delay3()
        {
            await Task.Delay(3000);
            return 3;
        }
    }

    [TestMethod]
    public async Task when_any_timeout()
    {
        var func = () => FuncWithTimeout();
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);

        async Task<string> FuncWithTimeout()
        {
            Task<string> task = SlowFunc();
            Task winner = await Task.WhenAny(task, Task.Delay(500));
            if (winner != task)
                throw new TimeoutException();
            return await task;
        }

        async Task<string> SlowFunc()
        {
            await Task.Delay(10000);
            return "foo";
        }
    }

    [TestMethod]
    public async Task when_all_exceptions()
    {
        Task task1 = Task.Run(() =>
        {
            throw null;
        });
        Task task2 = Task.Run(() =>
        {
            throw null;
        });
        Task all = Task.WhenAll(task1, task2);
        try
        {
            await all;
        }
        catch
        {
            Assert.AreEqual(2, all.Exception.InnerExceptions.Count);
        }
    }

    [TestMethod]
    public async Task when_all_results()
    {
        Task<int> task1 = Task.Run(() => 1);
        Task<int> task2 = Task.Run(() => 2);
        int[] results = await Task.WhenAll(task1, task2);
        CollectionAssert.AreEqual(new[] { 1, 2 }, results);
    }

    [TestMethod]
    public async Task when_all_example()
    {
        int totalSize = await GetTotalSize("https://ya.ru/ https://www.google.com/".Split());
        Console.WriteLine($"Total size {totalSize}");

        async Task<int> GetTotalSize(string[] uris)
        {
            IEnumerable<Task<int>> downloadTasks = uris.Select(
                async uri => (await new WebClient().DownloadDataTaskAsync(uri)).Length
            );

            int[] contentLengths = await Task.WhenAll(downloadTasks);
            return contentLengths.Sum();
        }
    }

    [TestMethod]
    public async Task with_timeout()
    {
        var func = () => SomeAsyncFunc().WithTimeout(TimeSpan.FromSeconds(1));
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
        async Task<string> SomeAsyncFunc()
        {
            await Task.Delay(10000);
            return "foo";
        }
    }

    [TestMethod]
    public async Task with_cancellation()
    {
        var cts = new CancellationTokenSource(500);
        var func = () => SomeAsyncFunc().WithCancellation(cts.Token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);

        async Task<string> SomeAsyncFunc()
        {
            await Task.Delay(10000);
            return "foo";
        }
    }

    [TestMethod]
    public async Task when_all_or_error()
    {
        Task<int> task1 = Task.Run (() => { throw null; return 42; } );
        Task<int> task2 = Task.Delay (5000).ContinueWith (ant => 53);
        var func = () => WhenAllOrError (task1, task2);
        await Assert.ThrowsExceptionAsync<NullReferenceException>(func);
        
        async Task<TResult[]> WhenAllOrError<TResult> (params Task<TResult>[] tasks)
        {
            var killJoy = new TaskCompletionSource<TResult[]>();
  
            foreach (var task in tasks)
                task.ContinueWith (ant =>
                {
                    if (ant.IsCanceled) 
                        killJoy.TrySetCanceled();
                    else if (ant.IsFaulted)
                        killJoy.TrySetException (ant.Exception.InnerException);
                });
    
            return await await Task.WhenAny (killJoy.Task, Task.WhenAll (tasks));    
        }
    }
}

public static class Extensions
{
    public static async Task<TResult> WithTimeout<TResult>(
        this Task<TResult> task,
        TimeSpan timeout
    )
    {
        Task winner = await Task.WhenAny(task, Task.Delay(timeout));
        if (winner != task)
            throw new TimeoutException();
        return await task; // Unwrap result/re-throw
    }

    public static Task<TResult> WithCancellation<TResult>(
        this Task<TResult> task,
        CancellationToken cancelToken
    )
    {
        var tcs = new TaskCompletionSource<TResult>();
        var reg = cancelToken.Register(() => tcs.TrySetCanceled());
        task.ContinueWith(ant =>
        {
            reg.Dispose();
            if (ant.IsCanceled)
                tcs.TrySetCanceled();
            else if (ant.IsFaulted)
                tcs.TrySetException(ant.Exception.InnerException);
            else
                tcs.TrySetResult(ant.Result);
        });
        return tcs.Task;
    }
}
