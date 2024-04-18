namespace Tests.ShaktiTanwar._2TasksPattern;

[TestClass]
public class AttachedTasksTests
{
    [TestMethod]
    public async Task detached_task()
    {
        Task parentTask = Task.Factory.StartNew(() =>
        {
            Console.WriteLine("Parent task started");

            Task childTask = Task.Factory.StartNew(() => {
                Console.WriteLine("Child task started");
            });
            Console.WriteLine("Parent task Finish");
        });
        parentTask.Wait();
        Console.WriteLine("Work Finished");
    }
    
    [TestMethod]
    public async Task attached_task()
    {
        Task parentTask = Task.Factory.StartNew(() =>
        {
            Console.WriteLine("Parent task started");

            Task childTask = Task.Factory.StartNew(() => {
                Console.WriteLine("Child task started");
            },TaskCreationOptions.AttachedToParent);
            Console.WriteLine("Parent task Finish");
        });
        parentTask.Wait(); // родительская задача также ждет дочернюю
        Console.WriteLine("Work Finished");
    }
}