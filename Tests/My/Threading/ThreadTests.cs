namespace Tests.My.Threading;

[TestClass]
public class ThreadTests
{
    [TestMethod]
    public async Task thread_example()
    {
        Thread t = new Thread (WriteY);          // Kick off a new thread
        t.Start();                               // running WriteY()
 
        // Simultaneously, do something on the main thread.
        for (int i = 0; i < 1000; i++)
        {
            Console.Write("x");
        }
    }
    
    static void WriteY()
    {
        for (int i = 0; i < 1000; i++)
        {
            Console.Write("y");
        }
    }
    
    [TestMethod]
    public async Task thread_local_vars()
    {
        new Thread (Go).Start();      // Call Go() on a new thread
        Go();                         // Call Go() on the main thread
    }
    
    static void Go()
    {
        // Declare and use a local variable - 'cycles'
        for (int cycles = 0; cycles < 5; cycles++)
        {
            Console.Write ('?');
        }
    }
    
    [TestMethod]
    public async Task thread_shared_var()
    {
        new Thread (Go1).Start();
        Go1();
    }
    
    bool done;
    
    void Go1() 
    {
        if (!done)
        {
            Console.WriteLine ("Done");
            done = true; 
            //Console.WriteLine ("Done");
        }
    }
    
    [TestMethod]
    public async Task thread_with_lock()
    {
        new Thread (GoWithLock).Start();
        GoWithLock();
    }
    
    readonly object locker = new object();
    
    void GoWithLock()
    {
        lock (locker)
        {
            if (!done)
            {
                Console.WriteLine ("Done"); 
                done = true;
            }
        }
    }
    
    [TestMethod]
    public async Task thread_join()
    {
        Thread t = new Thread (WriteY);
        t.Start();
        t.Join();
        Console.WriteLine ("Thread t has ended!");
    }
    
    [TestMethod]
    public async Task modify_shared_vars()
    {
        for (int i = 0; i < 10; i++)
        {
            new Thread (() => Console.Write(i)).Start();
        }
    }
    
    [TestMethod]
    public async Task thread_name()
    {
        Thread.CurrentThread.Name = "main";
        Thread worker = new Thread (Go);
        worker.Name = "worker";
        worker.Start();
        Go();
        
        static void Go()
        {
            Console.WriteLine ("Hello from " + Thread.CurrentThread.Name);
        }
    }
    
    [TestMethod]
    public async Task unhandled_exception_in_new_thread()
    {
        try
        {
            new Thread(() => throw null).Start();
        }
        catch (Exception ex)
        {
            // We'll never get here!
            Console.WriteLine ("Exception!");
        }
    }
    
    [TestMethod]
    public async Task handled_exception_in_new_thread()
    {
        new Thread(() =>
        {
            try
            {
                throw null;
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Exception!");
            }
        }).Start();
    }
}