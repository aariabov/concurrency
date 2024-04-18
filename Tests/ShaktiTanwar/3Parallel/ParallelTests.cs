using System.Collections.Concurrent;

namespace Tests.ShaktiTanwar._3Parallel;

[TestClass]
public class ParallelTests
{
    [TestMethod]
    public async Task parallel_invoke()
    {
        Parallel.Invoke(() => Console.WriteLine("Action 1"), () => Console.WriteLine("Action 2"));
        Console.WriteLine("Unblocked");
    }
    
    [TestMethod]
    public async Task parallel_on_tasks()
    {
        Task.Factory.StartNew(
            () => {
                Task.Factory.StartNew(() => Console.WriteLine("Action 1"), TaskCreationOptions.AttachedToParent);
                Task.Factory.StartNew(() => Console.WriteLine("Action 2"), TaskCreationOptions.AttachedToParent);
            }
        ).Wait();
    }
    
    [TestMethod]
    public async Task parallel_for()
    {
        var result = Parallel.For(1, 10, (i) => Console.WriteLine(i));
        Assert.IsTrue(result.IsCompleted);
    }
    
    [TestMethod]
    public async Task parallel_for_example()
    {
        int totalFiles = 0;
        var files = Directory.GetFiles("C:\\");
        Parallel.For(0, files.Length, (i) => {
            FileInfo fileInfo = new FileInfo(files[i]);
            if (fileInfo.CreationTime.Day == DateTime.Now.Day)
                Interlocked.Increment(ref totalFiles);
        });
        Console.WriteLine($"Total number of files in C: drive are {files.Count()} and {totalFiles} files were created today.");
    }
    
    [TestMethod]
    public async Task parallel_foreach_example()
    {
        var items = Enumerable.Range(1, 100);
        Parallel.ForEach(items, i => Console.WriteLine(i));
    }
    
    [TestMethod]
    public async Task max_degree_of_parallelism()
    {
        var result = Parallel.For(1, 100, new ParallelOptions { MaxDegreeOfParallelism = 4 }
            , (i) => Console.WriteLine($"TaskId {Task.CurrentId}, iter {i}"));
        Assert.IsTrue(result.IsCompleted);
    }
    
    [TestMethod]
    public async Task split_data()
    {
        OrderablePartitioner<Tuple<int,int>> orderablePartitioner= Partitioner.Create(1, 100);
            
        Parallel.ForEach(orderablePartitioner, (range, state) =>
        {
            var startRange = range.Item1;
            var endRange = range.Item2;
            Console.WriteLine($"Range execution finished on task {Task.CurrentId} with range {startRange}-{endRange}");
        });
    }
    
    [TestMethod]
    public async Task foreach_local_var()
    {
        var numbers = Enumerable.Range(1, 60);
        long sumOfNumbers = 0;
            
        Action<long> taskFinishedMethod = (taskResult) => {
            Console.WriteLine($"Sum at the end of all task iterations for task {Task.CurrentId} is {taskResult}" );
            Interlocked.Add(ref sumOfNumbers, taskResult);
        };

        Parallel.ForEach<int, long>(numbers, // collection of 60 number with each number having value equal to index
            () => 0, // method to initialize the local variable
            (j, loop, subtotal) => // Action performed on each iteration
            {
                subtotal += j; //Subtotal is Thread local variable
                return subtotal; // value to be passed to next iteration
            },
            taskFinishedMethod
        );
        Console.WriteLine($"The total of 60 numbers is {sumOfNumbers}");
        Assert.AreEqual(1830, sumOfNumbers);
    }
    
    [TestMethod]
    public async Task for_local_var()
    {
        long sumOfNumbers = 0;

        Action<long> taskFinishedMethod = (taskResult) => {
            Console.WriteLine($"Sum at the end of all task iterations for task {Task.CurrentId} is {taskResult}" );
            Interlocked.Add(ref sumOfNumbers, taskResult);
        };

        Parallel.For(1, 61, // collection of 60 number with each number having value equal to index
            () => 0, // method to initialize the local variable
            (j, loop, subtotal) => // Action performed on each iteration
            {
                subtotal += j; //Subtotal is Thread local variable
                return subtotal; // value to be passed to next iteration
            },
            taskFinishedMethod
        );
        Console.WriteLine($"The total of 60 numbers is {sumOfNumbers}");
        Assert.AreEqual(1830, sumOfNumbers);
    }
}