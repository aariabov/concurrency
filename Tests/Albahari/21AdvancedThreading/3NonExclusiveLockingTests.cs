using System.Diagnostics;
using System.Reactive.Disposables;

namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _3NonExclusiveLockingTests
{
    [TestMethod]
    public async Task semaphore_slim_example()
    {
        SemaphoreSlim sem = new SemaphoreSlim(3);
        var threads = new List<Thread>();
        for (int i = 1; i <= 5; i++)
        {
            var thread = new Thread(Enter);
            threads.Add(thread);
            thread.Start(i);
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        void Enter(object id)
        {
            Console.WriteLine(id + " wants to enter");
            sem.Wait();
            Console.WriteLine(id + " is in!"); // Only three threads
            Thread.Sleep(1000 * (int)id); // can be here at
            Console.WriteLine(id + " is leaving"); // a time.
            sem.Release();
        }
    }

    [TestMethod]
    public async Task async_semaphore_slim_example()
    {
        SemaphoreSlim _semaphore = new SemaphoreSlim(3);
        var tasks = new List<Task>();
        for (int i = 1; i <= 5; i++)
        {
            int local = i;
            var task = DownloadWithSemaphoreAsync(i, "http://someinvaliduri/" + i)
                .ContinueWith(c => Console.WriteLine("Finished download " + local));
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);

        async Task DownloadWithSemaphoreAsync(int id, string uri)
        {
            Console.WriteLine(id + " wants to enter");
            using (await _semaphore.EnterAsync())
            {
                Console.WriteLine(id + " is in!");
                await Task.Delay(100);
                Console.WriteLine(id + " is leaving");
            }
        }
    }

    [TestMethod]
    public async Task multi_thread_problem()
    {
        var numberList = new List<int>();
        var tasks = new List<Task>();
        for (int i = 1; i <= 10000; i++)
        {
            var local = i;
            var task = Task.Run(() =>
            {
                numberList.Add(local);
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"Number count {numberList.Count}");
    }

    [TestMethod]
    public async Task reader_writer_lock_slim_example()
    {
        ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
        List<int> items = new List<int>();
        Random rand = new Random();

        var tasks = new List<Task>();
        tasks.Add(Task.Run(() => Write()));
        tasks.Add(Task.Run(() => Write()));
        
        tasks.Add(Task.Run(() => Read()));
        tasks.Add(Task.Run(() => Read()));
        tasks.Add(Task.Run(() => Read()));
        
        await Task.Delay(2000);

        async Task Read()
        {
            while (true)
            {
                rw.EnterReadLock();
                Console.WriteLine($"Read thread {Environment.CurrentManagedThreadId}: {string.Join('-', items)}");
                await Task.Delay(100);
                rw.ExitReadLock();
            }
        }

        async Task Write()
        {
            while (true)
            {
                int newNumber = rand.Next(100);
                rw.EnterWriteLock();
                items.Add(newNumber);
                rw.ExitWriteLock();
                Console.WriteLine($"Write thread {Environment.CurrentManagedThreadId} added {newNumber}");
                await Task.Delay(100);
            }
        }
    }

    [TestMethod]
    public async Task reader_writer_lock_slim_upgradable()
    {
        ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
        List<int> items = new List<int>();
        Random rand = new Random();

        var threads = new List<Thread>();
        var readThread1 = new Thread(Read);
        threads.Add(readThread1);
        readThread1.Start();
        var readThread2 = new Thread(Read);
        threads.Add(readThread2);
        readThread2.Start();
        var readThread3 = new Thread(Read);
        threads.Add(readThread3);
        readThread3.Start();

        var writeThread1 = new Thread(Write);
        threads.Add(writeThread1);
        writeThread1.Start("A");
        var writeThread2 = new Thread(Write);
        threads.Add(writeThread2);
        writeThread2.Start("B");
        
        Thread.Sleep(1000);
        foreach (var thread in threads)
        {
            thread.Interrupt();
        }

        void Read()
        {
            while (true)
            {
                rw.EnterReadLock();
                foreach (int i in items)
                    Thread.Sleep(10);
                rw.ExitReadLock();
            }
        }

        void Write(object threadID)
        {
            while (true)
            {
                int newNumber = GetRandNum(10);
                rw.EnterUpgradeableReadLock();
                if (!items.Contains (newNumber))
                {
                    rw.EnterWriteLock();
                    items.Add (newNumber);
                    rw.ExitWriteLock();
                    Console.WriteLine ("Thread " + threadID + " added " + newNumber);
                }
                rw.ExitUpgradeableReadLock();
                Thread.Sleep(100);
            }
        }

        int GetRandNum(int max)
        {
            lock (rand)
                return rand.Next(max);
        }
    }

    [TestMethod]
    public async Task reader_writer_lock_slim_recursive()
    {
        var rw = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);
        rw.EnterReadLock();
        rw.EnterReadLock();
        rw.ExitReadLock();
        rw.ExitReadLock();

        rw.EnterWriteLock();
        rw.EnterReadLock();
        Console.WriteLine (rw.IsReadLockHeld);     // True
        Console.WriteLine (rw.IsWriteLockHeld);    // True
        rw.ExitReadLock();
        rw.ExitWriteLock();
    }
}

public static class Extensions
{
    public static async Task<IDisposable> EnterAsync(this SemaphoreSlim ss)
    {
        await ss.WaitAsync().ConfigureAwait(false);
        return Disposable.Create(() => ss.Release());
    }
}
