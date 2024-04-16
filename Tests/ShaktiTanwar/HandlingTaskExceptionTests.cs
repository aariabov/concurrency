namespace Tests.ShaktiTanwar;

[TestClass]
public class HandlingTaskExceptionTests
{
    [TestMethod]
    public async Task task_exception()
    {
        Task task = null;
        try
        {
            task = Task.Factory.StartNew(()=> throw new DivideByZeroException());
            task.Wait();
        }
        catch (AggregateException ex)
        {
            Console.WriteLine($"Task has finished with exception {ex.InnerException.Message}" );
        }
    }
    
    [TestMethod]
    public async Task multiple_task_exceptions()
    {
        Task taskA = Task.Factory.StartNew(()=> throw new DivideByZeroException());
        Task taskB = Task.Factory.StartNew(()=> throw new ArithmeticException());
        Task taskC = Task.Factory.StartNew(()=> throw new NullReferenceException());
        try
        {
            Task.WaitAll(taskA, taskB, taskC);
        }
        catch (AggregateException ex)
        {
            foreach (Exception innerException in ex.InnerExceptions)
            {
                Console.WriteLine(innerException.Message);
            }
        }
    }
    
    [TestMethod]
    public async Task multiple_task_callback_handler()
    {
        Task taskA = Task.Factory.StartNew(() => throw new DivideByZeroException());
        Task taskB = Task.Factory.StartNew(() => throw new ArithmeticException());
        Task taskC = Task.Factory.StartNew(() => throw new NullReferenceException());
        try
        {
            Task.WaitAll(taskA, taskB, taskC);
        }
        catch (AggregateException ex)
        {
            ex.Handle(innerException =>
            {
                Console.WriteLine(innerException.Message);
                return true;  
            });
        }
    }
}