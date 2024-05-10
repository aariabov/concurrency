namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _1ExclusiveLockingTests
{
    [TestMethod]
    public async Task thread_unsafe_problem()
    {
        bool done = false;
        void Go()
        {
            if (!done)
            {
                Console.WriteLine("Done");
                done = true;
            }
        }
        
        new Thread(Go).Start();
        Go();
    }
    
    [TestMethod]
    public async Task thread_safe_lock()
    {
        object locker = new object();
        bool done = false;
        void Go()
        {
            lock (locker)
            {
                if (!done)
                {
                    Console.WriteLine("Done");
                    done = true;
                } 
            }
        }
        
        new Thread(Go).Start();
        Go();
    }
    
    [TestMethod]
    public async Task thread_safe_monitor()
    {
        object locker = new object();
        bool done = false;
        void Go()
        {
            Monitor.Enter(locker);
            if (!done)
            {
                Console.WriteLine("Done");
                done = true;
            }
            Monitor.Exit(locker);
        }
        
        new Thread(Go).Start();
        Go();
    }
    
    [TestMethod]
    public async Task nested_locking()
    {
        object locker = new object();

        lock (locker)
        {
            AnotherMethod();
        }

        void AnotherMethod()
        {
            lock (locker) { Console.WriteLine ("Another method"); }
        }
    }
    
    [TestMethod]
    public async Task deadlock()
    {
        object locker1 = new object();
        object locker2 = new object();

        new Thread (() => {
            lock (locker1)
            {
                Thread.Sleep(1000);
                lock (locker2) { } // locker2 заблокировал нижний поток
            }
        }).Start();
        
        lock (locker2)
        {
            Thread.Sleep(1000);
            //lock (locker1) { } // locker1 заблокировал верхний поток
        }
    }
    
    [TestMethod]
    public async Task mutex()
    {
        using (var mutex = new Mutex (false, "oreilly.com OneAtATimeDemo"))
        {
            if (!mutex.WaitOne(TimeSpan.FromSeconds (3), false))
            {
                Console.WriteLine ("Another instance of the app is running. Bye!");
                return;
            }
    
            Console.WriteLine ("Running. Press Enter to exit");
        }
    }
}
