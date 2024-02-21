using System.Net;
using System.Net.Sockets;

namespace Tests.My.Tasks;

[TestClass]
public class TaskTests
{
    [TestMethod]
    public async Task task_run_запуск_в_другом_потоке()
    {
        var sut = new SutService();
        
        var method1ThreadId = sut.Method1();
        var method2ThreadId = await Task.Run(() => sut.Method2());
        
        Assert.IsInstanceOfType(method1ThreadId, typeof(int));
        Assert.IsInstanceOfType(method2ThreadId, typeof(int));
        Assert.AreNotEqual(method1ThreadId, method2ThreadId);
    }
    
    [TestMethod]
    public async Task task_factory_start_запуск_в_другом_потоке()
    {
        var sut = new SutService();
        
        var method1ThreadId = sut.Method1();
        var method2ThreadId = await Task.Factory.StartNew(() => sut.Method2());
        
        Assert.IsInstanceOfType(method1ThreadId, typeof(int));
        Assert.IsInstanceOfType(method2ThreadId, typeof(int));
        Assert.AreNotEqual(method1ThreadId, method2ThreadId);
    }
    
    [TestMethod]
    public async Task my_delay()
    {
        var sut = new SutService();
        
        await sut.MyDelay(100);
    }
    
    [TestMethod]
    public async Task my_timer_delay()
    {
        var sut = new SutService();
        
        Console.WriteLine($"Test: {Thread.CurrentThread.ManagedThreadId}");
        var task = sut.TimerDelay(100);
        await task;
    }
    
    [TestMethod]
    public async Task task_run_delay()
    {
        var sut = new SutService();
        
        Console.WriteLine($"Test: {Thread.CurrentThread.ManagedThreadId}");
        var task = sut.TaskRunDelay(100);
        await task;
    }
    
    [TestMethod]
    public async Task task_completion_source_example()
    {
        var sut = new SutService();
        // допустим, у нас во всем приложении используется устаревший WebClient
        // и мы хотим перейти на православный async/await
        var webClient = new WebClient();

        var bytes = await sut.DownloadDataAsync(webClient, "http://ya.ru/favicon.ico");
        Assert.IsTrue(bytes.Length > 0);
    }
}