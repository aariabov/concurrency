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
}