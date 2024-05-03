namespace Tests.ShaktiTanwar._5SyncPrimitives;

// TODO: не разобрался
[TestClass]
public class SlimLockTests
{
    [TestMethod]
    public async Task reader_writer_lock_slim()
    {
        Task writerTask = Task.Factory.StartNew(WriterTask);
        for (int i = 0; i < 3; i++)
        {
            Task readerTask = Task.Factory.StartNew(ReaderTask);
        }
        Thread.Sleep(10000);
    }
    static ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();
    static List<int> _list = new List<int>();
    static void WriterTask()
    {
        for (int i = 0; i < 4; i++)
        {
            try
            {
                _readerWriterLockSlim.EnterWriteLock();
                int random = new Random().Next(1, 10);
                Console.WriteLine($"Entered WriteLock on Task {Task.CurrentId}, Added {random}" );
                _list.Add(random);
                Console.WriteLine($"Exit WriteLock on Task {Task.CurrentId}");
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
            Thread.Sleep(1000);
        }
    }
    static void ReaderTask()
    {
        for (int i = 0; i < 2; i++)
        {
            try
            {
                _readerWriterLockSlim.EnterReadLock();
                var aggr = _list.Select(j => j.ToString()).Aggregate((a, b) => a + "," + b);
                Console.WriteLine($"Entered ReadLock on Task {Task.CurrentId}, Aggr {aggr}");
                Console.WriteLine($"Exiting ReadLock on Task {Task.CurrentId}");
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
            Thread.Sleep(1100);
        }
    }
    
    [TestMethod]
    public async Task throttler_using_semaphore_slim()
    {
        var range = Enumerable.Range(1, 12);
        SemaphoreSlim semaphore = new SemaphoreSlim(3, 3);
        range.AsParallel().AsOrdered().ForAll(i =>
        {
            try
            {
                semaphore.Wait();
                Console.WriteLine($"Index {i} making service call using Task {Task.CurrentId}");
                //Simulate Http call
                Thread.Sleep(1000);
                Console.WriteLine($"Index {i} releasing semaphore using Task {Task.CurrentId}");
            }
            finally
            {
                semaphore.Release();
            }
        });
    }
    
    [TestMethod]
    public async Task manual_reset_event_slim()
    {
        ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);
        Task signalOffTask = Task.Factory.StartNew(() => {
            while (true)
            {
                Thread.Sleep(3000);
                Console.WriteLine("Network is down");
                manualResetEvent.Reset();
            }
        });
        Task signalOnTask = Task.Factory.StartNew(() => {
            while (true)
            {
                Thread.Sleep(5000);
                Console.WriteLine("Network is Up");
                manualResetEvent.Set();
            }
        });
        for (int i = 0; i < 3; i++)
        {
            Parallel.For(0, 5, (j) => {
                Console.WriteLine($"Task with id {Task.CurrentId} waiting for network to be up");
                manualResetEvent.Wait();
                Console.WriteLine($"Task with id {Task.CurrentId} making service call");
                Thread.Sleep(10);
            });
            Thread.Sleep(3000);
        }
    }
    
    static string _serviceName = string.Empty;
    static Barrier serviceBarrier = new Barrier(5);
    static CountdownEvent serviceHost1CountdownEvent = new CountdownEvent(6);
    static CountdownEvent serviceHost2CountdownEvent = new CountdownEvent(6);
    static CountdownEvent finishCountdownEvent = new CountdownEvent(5);
    [TestMethod]
    public async Task countdown_event()
    {
        // Предположим, например, что нам необходимо получить данные от двух сервисов. Перед их извлечением из сервиса 1 его необходимо сперва запустить
        // и  уже после получения данных закрыть. Только когда сервис-1 закроется,
        // мы сможем запустить сервис 2 и извлечь из него данные. Данные должны
        // быть получены в кратчайшие сроки. 
        Task[] tasks = new Task[5];

        Task serviceManager = Task.Factory.StartNew(() =>
        {
            //Block until service name is set by any of thread
            while (string.IsNullOrEmpty(_serviceName))
                Thread.Sleep(1000);

            string serviceName = _serviceName;
            HostService(serviceName);
            //Now signal other threads to proceed
            serviceHost1CountdownEvent.Signal();

            serviceHost1CountdownEvent.Wait();

            //Block until service name is set by any of thread
            while (_serviceName != "Service2")
                Thread.Sleep(1000);
            //manualResetEventSlim.Set();
            Console.WriteLine($"All tasks completed for service {serviceName}." );
            CloseService(serviceName);
            HostService(_serviceName);
            serviceHost2CountdownEvent.Signal();

            serviceHost2CountdownEvent.Wait();

            finishCountdownEvent.Wait();
            CloseService(_serviceName);
            Console.WriteLine($"All tasks completed for service {_serviceName}." );
        });
        for (int i = 0; i < 5; ++i)
        {
            int j = i;
            tasks[j] = Task.Factory.StartNew(() =>
            {
                GetDataFromService1And2(j);
            });
        }
        Task.WaitAll(tasks);

        Console.WriteLine("Fetch completed");
    }

    private static void GetDataFromService1And2(int j)
    {
        _serviceName = "Service1";

        serviceHost1CountdownEvent.Signal();
        Console.WriteLine($"Task with id {Task.CurrentId} signaled countdown event and waiting for service to start");

        //Waiting for service to start
        serviceHost1CountdownEvent.Wait();
          
        Console.WriteLine($"Task with id {Task.CurrentId} fetching data from service " );
        serviceBarrier.SignalAndWait();
                  
        //change servicename
        _serviceName = "Service2";

        //Signal Countdown event
        serviceHost2CountdownEvent.Signal();
           
        Console.WriteLine($"Task with id {Task.CurrentId} signaled countdown event and waiting for service to start");
        serviceHost2CountdownEvent.Wait();

        Console.WriteLine($"Task with id {Task.CurrentId} fetching data from service ");
        serviceBarrier.SignalAndWait();
          
        //Signal Countdown event
        finishCountdownEvent.Signal();
    }
    private static void CloseService(string name)
    {
        Console.WriteLine($"Service {name} closed");
    }

    private static void HostService(string name)
    {
        Console.WriteLine($"Service {name} hosted");
    }
    
    [TestMethod]
    public async Task spin_lock()
    {
        Parallel.For(1, 5, (i) => SpinLock(i));
    }
    static List<int> _itemsList = new List<int>();
    static SpinLock _spinLock = new SpinLock();
    private static void SpinLock(int number)
    {
        bool lockTaken = false;
        try
        {
            Console.WriteLine($"Task {Task.CurrentId} Waiting for lock" );
            _spinLock.Enter(ref lockTaken);
            Console.WriteLine($"Task {Task.CurrentId} Updating list");
            _itemsList.Add(number);
        }
        finally
        {
            if (lockTaken)
            {
                Console.WriteLine($"Task {Task.CurrentId} Exiting Update");
                _spinLock.Exit(false);
            }
        }
    }
}