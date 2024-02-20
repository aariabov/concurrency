namespace Tests.My.Tasks;

[TestClass]
public class TaskRunVSTaskFactoryStartNew
{
    // https://code-maze.com/csharp-task-run-vs-task-factory-startnew/
    
    [TestMethod]
    public async Task task_factory_start_new_родительская_задача_ждет_дочернюю()
    {
        Task? innerTask = null;
        // Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);
        var outerTask = Task.Factory.StartNew(() =>
        {
            innerTask = new Task(() =>
            {
                Thread.Sleep(300);
                Console.WriteLine($"Внутренняя задача выполнена: {Thread.CurrentThread.ManagedThreadId}");
            }, TaskCreationOptions.AttachedToParent);

            innerTask.Start(TaskScheduler.Default);
            Console.WriteLine($"Внешняя задача выполнена: {Thread.CurrentThread.ManagedThreadId}");
        });

        outerTask.Wait();
        Console.WriteLine($"Выполнена ли внутренняя задача: {innerTask?.IsCompleted ?? false}");
        Console.WriteLine($"Главный поток завершен: {Thread.CurrentThread.ManagedThreadId}");
    }
    
    [TestMethod]
    public async Task task_run_родительская_задача_НЕ_ждет_дочернюю()
    {
        Task? innerTask = null;
        // Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        var outerTask = Task.Run(() =>
        {
            innerTask = new Task(() =>
            {
                Thread.Sleep(300);
                Console.WriteLine($"Внутренняя задача выполнена: {Thread.CurrentThread.ManagedThreadId}");
            }, TaskCreationOptions.AttachedToParent);

            innerTask.Start(TaskScheduler.Default);
            Console.WriteLine($"Внешняя задача выполнена: {Thread.CurrentThread.ManagedThreadId}");
        });

        outerTask.Wait();
        Console.WriteLine($"Выполнена ли внутренняя задача: {innerTask?.IsCompleted ?? false}");
        Console.WriteLine($"Главный поток завершен: {Thread.CurrentThread.ManagedThreadId}");
    }
    
    [TestMethod]
    public async Task task_factory_start_new_unwrap()
    {
        var task = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(500);
            return "Calculated Value";
        });

        Console.WriteLine(task.GetType()); // Task<Task<string>>

        var innerTask = task.Unwrap(); // надо руками разворачивать
        Console.WriteLine(innerTask.GetType()); // System.Threading.Tasks.UnwrapPromise`1[System.String]

        Console.WriteLine(innerTask.Result); // Calculated Value
    }
    
    [TestMethod]
    public async Task task_run_no_unwrap()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(500);
            return "Calculated Value";
        });
        Console.WriteLine(task.GetType()); // уже развернуто System.Threading.Tasks.UnwrapPromise`1[System.String]
    
        Console.WriteLine(task.Result); // Calculated Value
    }
    
    [TestMethod]
    public async Task task_run_wrong_state()
    {
        var tasks = new List<Task>();
        for (var i = 1; i < 4; i++)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(100);
                Console.WriteLine($"Iteration {i}"); // i всегда будет 4
            });
    
            tasks.Add(task);        
        }

        Task.WaitAll(tasks.ToArray());
    }
    
    [TestMethod]
    public async Task task_run_запоминаем_в_локальную_переменную_выделяем_память()
    {
        var tasks = new List<Task>();
        for (var i = 1; i < 4; i++)
        {
            var iteration = i;
            var task = Task.Run(async () =>
            {
                await Task.Delay(100);
                Console.WriteLine($"Iteration {iteration}");
            });
    
            tasks.Add(task);        
        }
        Task.WaitAll(tasks.ToArray());
    }
    
    [TestMethod]
    public async Task task_factory_start_new_with_object_state()
    {
        var tasks = new List<Task>();
        for (var i = 1; i < 4; i++)
        {
            var task = Task.Factory.StartNew(async (iteration) =>
                {
                    await Task.Delay(100);
                    Console.WriteLine($"Iteration {iteration}");
                }, i) // лучше в плане производительности
                .Unwrap();
    
            tasks.Add(task);        
        }
        Task.WaitAll(tasks.ToArray());
    }
}