namespace Tests.ShaktiTanwar._7Lazy;

[TestClass]
public class LazyTests
{
    [TestMethod]
    public async Task lazy()
    {
        Lazy<DataWrapper> lazyDataWrapper = new Lazy<DataWrapper>();
        Console.WriteLine("Lazy Object Created");
        Console.WriteLine("Now we want to access data");
        var data = lazyDataWrapper.Value.CachedData;

        Console.WriteLine($"Finishing up {string.Join(", ", data)}");
    }

    class DataWrapper
    {
        public DataWrapper()
        {
            CachedData = GetDataFromDatabase();
            Console.WriteLine("Object initialized");
        }

        public int[] CachedData { get; set; }

        private int[] GetDataFromDatabase()
        {
            Thread.Sleep(100);
            return new int[] { 1, 2, 3 };
        }
    }

    [TestMethod]
    public async Task lazy_for_multiple_threads()
    {
        // кешируем Ид первого потока
        Lazy<int> number = new Lazy<int>(() => Thread.CurrentThread.ManagedThreadId);

        Thread t1 = new Thread(
            () =>
                Console.WriteLine(
                    "number on t1 = {0} ThreadID = {1}",
                    number.Value,
                    Thread.CurrentThread.ManagedThreadId
                )
        );
        t1.Start();

        Thread t2 = new Thread(
            () =>
                Console.WriteLine(
                    "number on t2 = {0} ThreadID = {1}",
                    number.Value,
                    Thread.CurrentThread.ManagedThreadId
                )
        );
        t2.Start();

        Thread t3 = new Thread(
            () =>
                Console.WriteLine(
                    "number on t3 = {0} ThreadID = {1}",
                    number.Value,
                    Thread.CurrentThread.ManagedThreadId
                )
        );
        t3.Start();

        t1.Join();
        t2.Join();
        t3.Join();
    }

    [TestMethod]
    public async Task thread_local_for_multiple_threads()
    {
        // кешируем Ид для каждого потока
        ThreadLocal<int> threadLocalNumber = new ThreadLocal<int>(() => Thread.CurrentThread.ManagedThreadId);
        Thread t4 = new Thread(() => Console.WriteLine("threadLocalNumber on t4 = {0} ThreadID = {1}",
            threadLocalNumber.Value, Thread.CurrentThread.ManagedThreadId));
        t4.Start();

        Thread t5 = new Thread(() => Console.WriteLine("threadLocalNumber on t5 = {0} ThreadID = {1}",
            threadLocalNumber.Value, Thread.CurrentThread.ManagedThreadId));
        t5.Start();

        Thread t6 = new Thread(() => Console.WriteLine("threadLocalNumber on t6 = {0} ThreadID = {1}",
            threadLocalNumber.Value, Thread.CurrentThread.ManagedThreadId));
        t6.Start();

        t4.Join();
        t5.Join();
        t6.Join();
    }

    [TestMethod]
    public async Task lazy_method()
    {
        var testObj = new TestClass();
        Console.WriteLine("Lazy Object Created");
        var lazyDataWrapper = new Lazy<int[]>(() => testObj.GetDataFromDatabase());
        Console.WriteLine("Now we want to access data");
        var data = lazyDataWrapper.Value;
        Console.WriteLine($"Finishing up {string.Join(", ", data)}");
    }

    class TestClass
    {
        public int[] GetDataFromDatabase()
        {
            Console.WriteLine("Fetching data");
            Thread.Sleep(100);
            return new int[] { 1, 2, 3 };
        }
    }

    [TestMethod]
    public async Task lazy_cached()
    {
        var testObj = new TestClass1();
        Console.WriteLine("Creating Lazy object");
        var dataFetchLogic = new Func<int[]>(() => testObj.GetDataFromDatabase());
        // значение будет закешировано, но если добавить параметр, LazyThreadSafetyMode.PublicationOnly, то не будет
        var lazyDataWrapper = new Lazy<int[]>(dataFetchLogic);

        Console.WriteLine("Lazy Object Created");
        Console.WriteLine("Now we want to access data");
        int[] data = null;
        try
        {
            data = lazyDataWrapper.Value;
            Console.WriteLine("Data Fetched on Attempt 1");
        }
        catch (Exception)
        {
            Console.WriteLine("Exception 1");
        }

        try
        {
            testObj.counter++;
            data = lazyDataWrapper.Value;
            Console.WriteLine("Data Fetched on Attempt 2");
        }
        catch (Exception)
        {
            Console.WriteLine("Exception 2");
        }
        Console.WriteLine("Finishing up");
    }

    class TestClass1
    {
        public int counter = 0;
        public int[] CachedData { get; set; }

        public int[] GetDataFromDatabase()
        {
            if (counter == 0)
            {
                Console.WriteLine("Throwing exception");
                throw new Exception("Some Error has occured");
            }
            else
            {
                return new int[] { 1, 2, 3 };
            }
        }
    }

    static ThreadLocal<int> counter = new ThreadLocal<int>(() => 1);

    [TestMethod]
    public async Task thread_local()
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Factory.StartNew(
                () =>
                    Console.WriteLine(
                        $"Thread with id {Task.CurrentId} has counter value as {counter.Value}"
                    )
            );
        }
    }

    [TestMethod]
    public async Task lazy_initializer()
    {
        int[] _data = new int[] { };
        bool _initialized = false;

        object _locker = new object();
        Parallel.For(0, 10, (i) => Initializer());
        void Initializer()
        {
            Console.WriteLine($"Task with id {Task.CurrentId}");

            LazyInitializer.EnsureInitialized(
                ref _data,
                ref _initialized,
                ref _locker,
                () =>
                {
                    Console.WriteLine($"Task with id {Task.CurrentId} is Initializing data");
                    // Returns value that will be assigned in the ref parameter.
                    return new int[] { 1, 2, 3 };
                }
            );
        }
    }
}
