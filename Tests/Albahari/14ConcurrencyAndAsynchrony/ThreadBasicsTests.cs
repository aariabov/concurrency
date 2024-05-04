namespace Tests.Albahari._14ConcurrencyAndAsynchrony;

[TestClass]
public class ThreadBasicsTests
{
    [TestMethod]
    public async Task parallel_thread_example()
    {
        Thread t = new Thread(WriteY); // Kick off a new thread
        t.Name = "test";
        t.Start(); // running WriteY()

        // Simultaneously, do something on the main thread.
        for (int i = 0; i < 100; i++)
        {
            Console.Write("x");
            Thread.Sleep(0);
        }

        void WriteY()
        {
            for (int i = 0; i < 100; i++)
            {
                Console.Write("y");
                Thread.Sleep(0);
            }
        }
    }

    [TestMethod]
    public async Task thread_join()
    {
        Thread t = new Thread(Go);
        t.Start();
        //t.Join();
        Console.WriteLine("Thread t has ended!");

        void Go()
        {
            for (int i = 0; i < 1000; i++)
                Console.Write("y");
        }
    }

    [TestMethod]
    public async Task local_state()
    {
        new Thread(Go).Start(); // Call Go() on a new thread
        Go(); // Call Go() on the main thread
        Console.Write(" ");

        void Go()
        {
            // Declare and use a local variable - 'cycles'
            for (int cycles = 0; cycles < 5; cycles++)
                Console.Write(cycles);
        }
    }

    [TestMethod]
    public async Task shared_state_unsafe()
    {
        bool _done = false;

        new Thread(Go).Start();
        Go();

        void Go()
        {
            if (!_done)
            {
                _done = true;
                Console.WriteLine("Done");
            }
        }
    }

    [TestMethod]
    public async Task shared_state_with_fields_unsafe()
    {
        var tt = new ThreadTest();
        new Thread(tt.Go).Start();
        tt.Go();
    }

    private class ThreadTest
    {
        bool _done;

        public void Go()
        {
            if (!_done)
            {
                _done = true;
                Console.WriteLine("Done");
            }
        }
    }

    [TestMethod]
    public async Task shared_state_with_closure_unsafe()
    {
        bool done = false;
        ThreadStart action = () =>
        {
            if (!done)
            {
                done = true;
                Console.WriteLine("Done");
            }
        };
        new Thread(action).Start();
        action();
    }

    [TestMethod]
    public async Task shared_state_with_static_unsafe_problem()
    {
        ThreadTest1.Main1();
    }

    private class ThreadTest1
    {
        static bool _done; // Static fields are shared between all threads

        // in the same application domain.
        public static void Main1()
        {
            new Thread(Go).Start();
            Go();
        }

        static void Go()
        {
            if (!_done)
            {
                Console.WriteLine("Done");
                _done = true;
            }
        }
    }

    [TestMethod]
    public async Task shared_state_with_lock_safe()
    {
        ThreadSafe.Main1();
    }

    class ThreadSafe
    {
        static bool _done;
        static readonly object _locker = new object();

        public static void Main1()
        {
            new Thread(Go).Start();
            Go();
        }

        static void Go()
        {
            lock (_locker)
            {
                if (!_done)
                {
                    Console.WriteLine("Done");
                    _done = true;
                }
            }
        }
    }

    [TestMethod]
    public async Task passing_in_data_with_a_lambda_expression()
    {
        Thread t = new Thread(() => Print("Hello from t!"));
        t.Start();

        void Print(string message) => Console.WriteLine(message);
    }

    [TestMethod]
    public async Task multi_statement_lambda()
    {
        new Thread(() =>
        {
            Console.WriteLine("I'm running on another thread!");
            Console.WriteLine("This is so easy!");
        }).Start();
    }

    [TestMethod]
    public async Task lambdas_and_captured_variables_unsafe_problem()
    {
        for (int i = 0; i < 10; i++)
            new Thread(() => Console.Write(i)).Start();
    }

    [TestMethod]
    public async Task lambdas_and_captured_variables_safe()
    {
        for (int i = 0; i < 10; i++)
        {
            int temp = i;
            new Thread(() => Console.Write(temp)).Start();
        }
    }

    [TestMethod]
    public async Task exception_handling_wrong_place_problem()
    {
        try
        {
            new Thread(Go).Start();
        }
        catch (Exception ex)
        {
            // We'll never get here!
            Console.WriteLine("Exception!");
        }

        static void Go()
        {
            throw null;
        } // Throws a NullReferenceException
    }

    [TestMethod]
    public async Task exception_handling_right_place()
    {
        new Thread(Go).Start();

        void Go()
        {
            try
            {
                throw null; // The NullReferenceException will get caught below
            }
            catch (Exception ex)
            {
                //Typically log the exception, and/or signal another thread
                // that we've come unstuck
                Console.WriteLine("Caught!");
            }
        }
    }

    [TestMethod]
    public async Task basic_signaling()
    {
        var signal = new ManualResetEvent(false);

        new Thread(() =>
        {
            Console.WriteLine("Waiting for signal...");
            signal.WaitOne(); // ждем сигнала
            signal.Dispose();
            Console.WriteLine("Got signal!");
        }).Start();

        Thread.Sleep(2000);
        signal.Set(); // сигнал
    }

    [TestMethod]
    public async Task thread_pool_old_way()
    {
        ThreadPool.QueueUserWorkItem(notUsed => Console.WriteLine("Hello, old-school"));
    }

    [TestMethod]
    public async Task thread_pool()
    {
        Task.Run(() => Console.WriteLine("Hello from the thread pool"));
    }
}
