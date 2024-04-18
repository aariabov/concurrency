using System.ComponentModel;

namespace Tests.ShaktiTanwar._2TasksPattern;

[TestClass]
public class CreatingThreadTests
{
    [TestMethod]
    public async Task thread_start_example()
    {
        Console.WriteLine($"Main ThreadId {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"Main IsBackground: {Thread.CurrentThread.IsBackground}");
        Console.WriteLine($"Main ApartmentState {Thread.CurrentThread.GetApartmentState()}");

        var thread = new Thread(() =>
        {
            Console.WriteLine($"Child ThreadId {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Child IsBackground: {Thread.CurrentThread.IsBackground}");
            Console.WriteLine($"Child ApartmentState {Thread.CurrentThread.GetApartmentState()}");
        });
        thread.Start();
        
        Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, thread.ManagedThreadId);
    }
    
    [TestMethod]
    public async Task thread_start_with_param_example()
    {
        Console.WriteLine($"Main ThreadId {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"Main IsBackground: {Thread.CurrentThread.IsBackground}");
        Console.WriteLine($"Main ApartmentState {Thread.CurrentThread.GetApartmentState()}");

        var thread = new Thread((par) =>
        {
            Console.WriteLine($"Param {par}");
            Console.WriteLine($"Child ThreadId {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Child IsBackground: {Thread.CurrentThread.IsBackground}");
            Console.WriteLine($"Child ApartmentState {Thread.CurrentThread.GetApartmentState()}");
        });
        thread.Start(42);
        
        Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, thread.ManagedThreadId);
    }
    
    [TestMethod]
    public async Task thread_pool_example()
    {
        Console.WriteLine($"Main ThreadId {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"Main IsBackground: {Thread.CurrentThread.IsBackground}");
        Console.WriteLine($"Main IsThreadPoolThread {Thread.CurrentThread.IsThreadPoolThread}");
        Console.WriteLine($"Main ApartmentState {Thread.CurrentThread.GetApartmentState()}");

        ThreadPool.QueueUserWorkItem((state) => {
            Console.WriteLine($"State {state}");
            Console.WriteLine($"Child ThreadId {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Child IsBackground: {Thread.CurrentThread.IsBackground}");
            Console.WriteLine($"Child IsThreadPoolThread {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Child ApartmentState {Thread.CurrentThread.GetApartmentState()}");
        }, 42);
    }
    
    [TestMethod]
    public async Task background_worker_example()
    {
        Helpers.PrintThreadInfo("Main");
        
        var resetEvent = new AutoResetEvent(false);
        var backgroundWorker = new BackgroundWorker();
        backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.WorkerSupportsCancellation = true;

        backgroundWorker.DoWork += (sender, e) =>
        {
            Helpers.PrintThreadInfo("Worker");
            var worker = sender as BackgroundWorker;
            for (var i = 1; i <= 10; i++)
            {
                Console.WriteLine($"Iteration {i}");
                worker.ReportProgress(i * 10);
                Thread.Sleep(10);
            }

            e.Result = "OK";
            resetEvent.Set();
        };
        backgroundWorker.ProgressChanged += (sender, e) => Console.WriteLine($"{e.ProgressPercentage}% completed");
        backgroundWorker.RunWorkerCompleted += (sender, e) => Console.WriteLine($"Result {e.Result}");
        backgroundWorker.RunWorkerAsync();
        resetEvent.WaitOne();
    }
    
    [TestMethod]
    public async Task background_worker_cancel()
    {
        Helpers.PrintThreadInfo("Main");
        
        var resetEvent = new AutoResetEvent(false);
        var backgroundWorker = new BackgroundWorker();
        backgroundWorker.WorkerReportsProgress = true;
        backgroundWorker.WorkerSupportsCancellation = true;

        backgroundWorker.DoWork += (sender, e) =>
        {
            Helpers.PrintThreadInfo("Worker");
            var worker = sender as BackgroundWorker;
            for (var i = 1; i <= 10; i++)
            {
                if (!worker.CancellationPending)
                {
                    Console.WriteLine($"Iteration {i}");
                    worker.ReportProgress(i * 10);
                    Thread.Sleep(1000);
                }
                else
                {
                    e.Cancel = true;
                    worker.CancelAsync();
                }
            }

            e.Result = "OK";
            resetEvent.Set();
        };
        backgroundWorker.ProgressChanged += (sender, e) => Console.WriteLine($"{e.ProgressPercentage}% completed");
        backgroundWorker.RunWorkerCompleted += (sender, e) => Console.WriteLine($"Cancelled {e.Cancelled}");
        backgroundWorker.RunWorkerAsync();
        
        Thread.Sleep(3000);
        backgroundWorker.CancelAsync();
        resetEvent.WaitOne();
    }
    
    [TestMethod]
    public async Task background_worker_error()
    {
        var backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += (sender, e) => throw new Exception("Yps");
        backgroundWorker.RunWorkerCompleted += (sender, e) => Console.WriteLine($"Error {e.Error.Message}");
        
        backgroundWorker.RunWorkerAsync();
    }
}