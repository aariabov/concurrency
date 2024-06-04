namespace Tests.Albahari._22ParallelProgramming;

[TestClass]
public class _3Tasks
{
    [TestMethod]
    public async Task AttachedToParent()
    {
        Task parent = Task.Factory.StartNew (() =>
        {
            Console.WriteLine ("I am a parent");
  
            Task.Factory.StartNew (() => // родительская задача не будет дожидаться этой
            {
                Thread.Sleep(1000);
                Console.WriteLine ("I am detached");
            });
  
            Task.Factory.StartNew (() => // родительская задача будет ждать дочернюю задачу
            {
                Console.WriteLine ("I am a child");
            }, TaskCreationOptions.AttachedToParent);
        });

        parent.Wait();
        Console.WriteLine ("Parent completed");
    }
    
    [TestMethod]
    public async Task child_task_exception()
    {
        TaskCreationOptions attachedToParent = TaskCreationOptions.AttachedToParent;
        var parent = Task.Factory.StartNew (() => 
        {
            Console.WriteLine ("I am a parent");
            Task.Factory.StartNew (() =>   // Child
            {
                Console.WriteLine ("I am a child");
                Task.Factory.StartNew (() =>
                {
                    Console.WriteLine ("I am a parent");
                    throw null;
                }, attachedToParent);
            }, attachedToParent);
        });

        Assert.ThrowsException<AggregateException>(() => parent.Wait());
    }
    
    [TestMethod]
    public async Task wait_all_tasks_manual()
    {
        await Assert.ThrowsExceptionAsync<AggregateException>(() => SomeFunction());

        Task SomeFunction()
        {
            var task1 = Task.Run(() => throw null);
            var task2 = Task.Run(() => throw null);
            var task3 = Task.Run(() => throw null);
            
            var exceptions = new List<Exception>();
            try { task1.Wait(); } catch (AggregateException ex) { exceptions.Add (ex); }
            try { task2.Wait(); } catch (AggregateException ex) { exceptions.Add (ex); }
            try { task3.Wait(); } catch (AggregateException ex) { exceptions.Add (ex); }
            if (exceptions.Count > 0) 
                throw new AggregateException (exceptions);

            return Task.CompletedTask;
        }
    }
    
    [TestMethod]
    public async Task wait_all_tasks_wait_all()
    {
        Assert.ThrowsException<AggregateException>(() => SomeFunction());

        void SomeFunction()
        {
            var task1 = Task.Run(() => throw null);
            var task2 = Task.Run(() => throw null);
            var task3 = Task.Run(() => throw null);

            Task.WaitAll(task1, task2, task3);
        }
    }
    
    [TestMethod]
    public async Task continue_with_depend_on_task_result()
    {
        var task1 = Task.Factory.StartNew (() => 42);

        var error = task1.ContinueWith(ant => Console.Write(ant.Exception),
            TaskContinuationOptions.OnlyOnFaulted);

        var ok = task1.ContinueWith(ant => Console.Write($"Result {ant.Result}"),
            TaskContinuationOptions.NotOnFaulted);

        ok.Wait();
    }
    
    [TestMethod]
    public async Task multiple_continuations()
    {
        var t = Task.Factory.StartNew(() => Thread.Sleep (100));

        var c1 = t.ContinueWith(ant => Console.Write ("X"));
        var c2 = t.ContinueWith(ant => Console.Write ("Y"));

        Task.WaitAll(c1, c2);
    }
    
    [TestMethod]
    public async Task custom_factory()
    {
        var factory = new TaskFactory (
            TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent,
            TaskContinuationOptions.None);

        Task task1 = factory.StartNew (() => Console.WriteLine("foo"));
        Task task2 = factory.StartNew (() => Console.WriteLine("far"));
    }
}
