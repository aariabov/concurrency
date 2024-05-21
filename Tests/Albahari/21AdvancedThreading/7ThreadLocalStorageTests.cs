using System.Text;

namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _7ThreadLocalStorageTests
{
    [ThreadStatic]
    static int _x;

    [TestMethod]
    public async Task thread_static_example()
    {
        new Thread(() =>
        {
            Thread.Sleep(1000);
            _x++;
            Console.WriteLine(_x);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(2000);
            _x++;
            Console.WriteLine(_x);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(3000);
            _x++;
            Console.WriteLine(_x);
        }).Start();
        Thread.Sleep(3500);
    }

    [TestMethod]
    public async Task thread_local_example()
    {
        ThreadLocal<int> _x = new ThreadLocal<int>(() => 3);
        new Thread(() =>
        {
            Thread.Sleep(1000);
            _x.Value++;
            Console.WriteLine(_x);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(2000);
            _x.Value++;
            Console.WriteLine(_x);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(3000);
            _x.Value++;
            Console.WriteLine(_x);
        }).Start();
        Thread.Sleep(3500);
    }

    [TestMethod]
    public async Task get_set_data_example()
    {
        var test = new Test();
        new Thread(() =>
        {
            Thread.Sleep(1000);
            test.SecurityLevel++;
            Console.WriteLine(test.SecurityLevel);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(2000);
            test.SecurityLevel++;
            Console.WriteLine(test.SecurityLevel);
        }).Start();
        new Thread(() =>
        {
            Thread.Sleep(3000);
            test.SecurityLevel++;
            Console.WriteLine(test.SecurityLevel);
        }).Start();
        Thread.Sleep(3500);
    }

    class Test
    {
        // The same LocalDataStoreSlot object can be used across all threads.
        LocalDataStoreSlot _secSlot = Thread.GetNamedDataSlot("securityLevel");

        // This property has a separate value on each thread.
        public int SecurityLevel
        {
            get
            {
                object data = Thread.GetData(_secSlot);
                return data == null ? 0 : (int)data; // null == uninitialized
            }
            set { Thread.SetData(_secSlot, value); }
        }
    }

    [TestMethod]
    public async Task async_local_example()
    {
        AsyncLocal<string> _asyncLocalTest = new AsyncLocal<string>();
        Console.WriteLine($"Current Thread ID {Thread.CurrentThread.ManagedThreadId}");
        _asyncLocalTest.Value = "test";

        await Task.Delay(1000);

        Console.WriteLine($"Current Thread ID {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine(_asyncLocalTest.Value);
    }

    [TestMethod]
    public async Task async_local_concurrent()
    {
        AsyncLocal<string> asyncLocalTest = new AsyncLocal<string>();
        new Thread(() => Test("one")).Start();
        new Thread(() => Test("two")).Start();
        Thread.Sleep(1500);

        async void Test(string value)
        {
            asyncLocalTest.Value = value;
            await Task.Delay(1000);
            Console.WriteLine(value + " " + asyncLocalTest.Value);
        }
    }

    [TestMethod]
    public async Task async_local_inherited_value()
    {
        AsyncLocal<string> _asyncLocalTest = new AsyncLocal<string>();
        _asyncLocalTest.Value = "test";
        new Thread(AnotherMethod).Start();

        void AnotherMethod() => Console.WriteLine(_asyncLocalTest.Value);
    }

    [TestMethod]
    public async Task async_local_inherited_value1()
    {
        AsyncLocal<string> _asyncLocalTest = new AsyncLocal<string>();
        _asyncLocalTest.Value = "test";
        var t = new Thread(AnotherMethod);
        t.Start();
        t.Join();
        Console.WriteLine(_asyncLocalTest.Value);

        void AnotherMethod() => _asyncLocalTest.Value = "ha-ha!";
    }

    [TestMethod]
    public async Task async_local_inherited_problem()
    {
        AsyncLocal<StringBuilder> _asyncLocalTest = new AsyncLocal<StringBuilder>();
        _asyncLocalTest.Value = new StringBuilder("test");
        var t = new Thread(AnotherMethod);
        t.Start();
        t.Join();
        Console.WriteLine(_asyncLocalTest.Value.ToString());

        void AnotherMethod() => _asyncLocalTest.Value.Append(" ha-ha!");
    }
}
