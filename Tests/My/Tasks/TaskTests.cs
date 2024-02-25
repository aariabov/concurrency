using System.Net;
using System.Net.Sockets;
using Nito.AsyncEx;

namespace Tests.My.Tasks;

[TestClass]
public class TaskTests
{
    [TestMethod]
    public async Task task_run_запуск_в_другом_потоке()
    {
        var sut = new SutService();
        
        var method1ThreadId = sut.Method1();
        var method2ThreadId = await Task.Run(() => sut.Method2());
        
        Assert.IsInstanceOfType(method1ThreadId, typeof(int));
        Assert.IsInstanceOfType(method2ThreadId, typeof(int));
        Assert.AreNotEqual(method1ThreadId, method2ThreadId);
    }
    
    [TestMethod]
    public async Task task_factory_start_запуск_в_другом_потоке()
    {
        var sut = new SutService();
        
        var method1ThreadId = sut.Method1();
        var method2ThreadId = await Task.Factory.StartNew(() => sut.Method2());
        
        Assert.IsInstanceOfType(method1ThreadId, typeof(int));
        Assert.IsInstanceOfType(method2ThreadId, typeof(int));
        Assert.AreNotEqual(method1ThreadId, method2ThreadId);
    }
    
    [TestMethod]
    public async Task my_delay()
    {
        var sut = new SutService();
        
        await sut.MyDelay(100);
    }
    
    [TestMethod]
    public async Task my_timer_delay()
    {
        var sut = new SutService();
        
        Console.WriteLine($"Test: {Thread.CurrentThread.ManagedThreadId}");
        var task = sut.TimerDelay(100);
        await task;
    }
    
    [TestMethod]
    public async Task task_run_delay()
    {
        var sut = new SutService();
        
        Console.WriteLine($"Test: {Thread.CurrentThread.ManagedThreadId}");
        var task = sut.TaskRunDelay(100);
        await task;
    }
    
    [TestMethod]
    public async Task task_completion_source_example()
    {
        var sut = new SutService();
        // допустим, у нас во всем приложении используется устаревший WebClient
        // и мы хотим перейти на православный async/await
        var webClient = new WebClient();

        var bytes = await sut.DownloadDataAsync(webClient, "http://ya.ru/favicon.ico");
        Assert.IsTrue(bytes.Length > 0);
    }
    
    [TestMethod]
    public async Task task_with_timeout()
    {
        var sut = new SutService();
        
        var task = Task.Run(() => 42);
        var result = await sut.WithTimeout(task, 100);
        Assert.AreEqual(42, result);
    }
    
    [TestMethod]
    public async Task task_timeout_exception()
    {
        var sut = new SutService();
        var task = Task.Run(async () =>
        {
            await Task.Delay(200);
            return 42;
        });
        
        var func = () => sut.WithTimeout(task, 100);
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
    }
    
    [TestMethod]
    public async Task task_delay_cancel()
    {
        var sut = new SutService();
        var cancelTokenSource = new CancellationTokenSource();
        cancelTokenSource.CancelAfter(200);
        var token = cancelTokenSource.Token;

        Task task = null;
        var func = () => task = sut.LongCalc(token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
        Assert.AreEqual(task!.Status, TaskStatus.Canceled);
    }
    
    [TestMethod]
    public async Task task_with_my_delay_cancel()
    {
        var sut = new SutService();
        var cancelTokenSource = new CancellationTokenSource(200);

        Task task = null;
        var func = () => task = sut.LongCalcWithMyDelay(cancelTokenSource.Token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
        Assert.AreEqual(task!.Status, TaskStatus.Canceled);
    }
    
    [TestMethod]
    public async Task task_progress()
    {
        var sut = new SutService();

        await sut.ProgressTask(new Progress<int>(p => Console.WriteLine($"{p}%")));
    }
    
    [TestMethod]
    public async Task dead_lock()
    {
        AsyncContext.Run(async () =>
        {
            Console.WriteLine($"Test start: {Thread.CurrentThread.ManagedThreadId}");
            var sut = new SutService();

            Task task = sut.WaitAsync();
            // Синхронное ожидание блокирует текущий поток и после await возникает deadlock,
            // тк работа не может быть продолжена в этом потоке
            //task.Wait();
            await task;
            Console.WriteLine($"Test end: {Thread.CurrentThread.ManagedThreadId}");
        });
    }
    
    [TestMethod]
    public async Task dead_lock_result()
    {
        AsyncContext.Run(async () =>
        {
            Console.WriteLine($"Test start: {Thread.CurrentThread.ManagedThreadId}");
            var sut = new SutService();

            var task = sut.GetResultAsync();
            // Синхронное ожидание блокирует текущий поток и после await возникает deadlock,
            // тк работа не может быть продолжена в этом потоке
            var result = task.Result;
            //var result = await task;
            Console.WriteLine($"Test end: {Thread.CurrentThread.ManagedThreadId}");
        });
    }
    
    [TestMethod]
    public async Task lock_example()
    {
        var account = new Account(1000);
        var tasks = new Task[100];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => Update(account));
        }
        await Task.WhenAll(tasks);
        
        Assert.AreEqual(2000, account.GetBalance());
    }
    
    static void Update(Account account)
    {
        decimal[] amounts = {0, 2, -3, 6, -2, -1, 8, -5, 11, -6}; // в сумме 10
        foreach (var amount in amounts)
        {
            if (amount >= 0)
            {
                account.Credit(amount);
            }
            else
            {
                account.Debit(Math.Abs(amount));
            }
        }
    }
    
    [TestMethod]
    public async Task заранее_выделено_несколько_объектов_task()
    {
        // в .NET заранее выделено несколько объектов Task
        var task1 = Task.FromResult(true);
        var task2 = Task.FromResult(true);
        Assert.AreSame(task1, task2);
    }
}
