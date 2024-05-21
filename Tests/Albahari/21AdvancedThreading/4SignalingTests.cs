namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _4SignalingTests
{
    [TestMethod]
    public async Task event_wait_handle_example()
    {
        EventWaitHandle _waitHandle = new AutoResetEvent(false);
        new Thread(Waiter).Start();
        Thread.Sleep(1000); // Pause for a second...
        _waitHandle.Set(); // Wake up the Waiter.

        void Waiter()
        {
            Console.WriteLine("Waiting...");
            _waitHandle.WaitOne(); // Wait for notification
            Console.WriteLine("Notified");
        }
    }

    [TestMethod]
    public async Task event_wait_handle_two_way_signaling()
    {
        EventWaitHandle _ready = new AutoResetEvent(false);
        EventWaitHandle _go = new AutoResetEvent(false);
        object _locker = new object();
        string _message = null;

        new Thread(Work).Start();

        _ready.WaitOne(); // First wait until worker is ready
        lock (_locker)
            _message = "ooo";
        _go.Set(); // Tell worker to go

        _ready.WaitOne();
        lock (_locker)
            _message = "ahhh"; // Give the worker another message
        _go.Set();

        _ready.WaitOne();
        lock (_locker)
            _message = null; // Signal the worker to exit
        _go.Set();

        void Work()
        {
            while (true)
            {
                _ready.Set(); // Indicate that we're ready
                _go.WaitOne(); // Wait to be kicked off...
                lock (_locker)
                {
                    if (_message == null)
                        return; // Gracefully exit
                    Console.WriteLine(_message);
                }
            }
        }
    }

    [TestMethod]
    public async Task count_down_event_example()
    {
        var countdown = new CountdownEvent(3); // Initialize with "count" of 3.

        new Thread(SaySomething).Start("I am thread 1");
        new Thread(SaySomething).Start("I am thread 2");
        new Thread(SaySomething).Start("I am thread 3");

        countdown.Wait(); // Blocks until Signal has been called 3 times
        Console.WriteLine("All threads have finished speaking!");

        void SaySomething(object thing)
        {
            Thread.Sleep(1000);
            Console.WriteLine(thing);
            countdown.Signal();
        }
    }

    [TestMethod]
    public async Task wait_handles_and_continuations()
    {
        var starter = new ManualResetEvent(false);

        var reg = ThreadPool.RegisterWaitForSingleObject(starter, Go, "Some Data", -1, true);
        Thread.Sleep(1000);
        Console.WriteLine("Signaling worker...");
        starter.Set();
        reg.Unregister(starter); // Clean up when we’re done.

        void Go(object data, bool timedOut)
        {
            Console.WriteLine("Started - " + data);
            // Perform task...
        }
    }
}
