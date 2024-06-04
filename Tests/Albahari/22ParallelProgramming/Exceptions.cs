namespace Tests.Albahari._22ParallelProgramming;

[TestClass]
public class Exceptions
{
    [TestMethod]
    public async Task aggregate_exception()
    {
        try
        {
            var query = from i in ParallelEnumerable.Range (0, 1000000)
                select 100 / i;
            query.Take(100).ToArray();
        }
        catch (AggregateException aex)
        {
            foreach (Exception ex in aex.InnerExceptions)
                Console.WriteLine (ex.Message);
        }
    }
    
    [TestMethod]
    public async Task flatten()
    {
        try
        {
            var query = from i in ParallelEnumerable.Range (0, 1000000)
                select 100 / i;
            query.Take(100).ToArray();
        }
        catch (AggregateException aex)
        {
            foreach (Exception ex in aex.Flatten().InnerExceptions)
                Console.WriteLine (ex.Message);
        }
    }
    
    [TestMethod]
    public async Task handle()
    {
        var parent = Task.Factory.StartNew (() => 
        {
            int[] numbers = { 0 };
            var childFactory = new TaskFactory(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.None);
  
            childFactory.StartNew (() => 5 / numbers[0]);   // Division by zero
            childFactory.StartNew (() => numbers [1]);      // Index out of range
            childFactory.StartNew (() => { throw null; });  // Null reference
        });

        try
        {
            Func();
        }
        catch (AggregateException e)
        {
            foreach (Exception ex in e.Flatten().InnerExceptions)
                Console.WriteLine (ex.Message);
        }

        void Func()
        {
            try { parent.Wait(); }
            catch (AggregateException aex)
            {
                aex.Flatten().Handle(ex =>
                {
                    if (ex is DivideByZeroException)
                    {
                        Console.WriteLine ("Divide by zero");
                        return true;                           // This exception is "handled"
                    }
                    if (ex is IndexOutOfRangeException)
                    {
                        Console.WriteLine ("Index out of range");
                        return true;                           // This exception is "handled"   
                    }
                    return false;    // All other exceptions will get rethrown
                });
            }
        }
    }
}