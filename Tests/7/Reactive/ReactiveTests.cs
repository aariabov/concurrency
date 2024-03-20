using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;

namespace Tests.Reactive;

[TestClass]
public class ReactiveTests
{
    [TestMethod]
    public async Task observable_timeout_return_value()
    {
        var stub = new SuccessHttpServiceStub();
        var my = new MyTimeoutClass(stub);
        var result = await my.GetStringWithTimeout("http://www.example.com/")
            .SingleAsync();
        Assert.AreEqual("stub", result);
    }
    
    [TestMethod]
    public async Task observable_timeout_error()
    {
        var stub = new FailureHttpServiceStub();
        var my = new MyTimeoutClass(stub);
        await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
        {
            await my.GetStringWithTimeout("http://www.example.com/")
                .SingleAsync();
        });
    }
    
    [TestMethod]
    public void observable_timeout_with_scheduler_return_value()
    {
        var scheduler = new TestScheduler();
        var stub = new SuccessHttpServiceStubWithScheduler
        {
            Scheduler = scheduler,
            // выполнение этого модульного теста не занимает 0,5 секунды;
            // Полусекундная задержка существует только в виртуальном времени
            Delay = TimeSpan.FromSeconds(0.5),
        };
        var my = new MyTimeoutClass(stub);
        string result = null;
        
        my.GetStringWithTimeout("http://www.example.com/", scheduler)
            .Subscribe(r => { result = r; });
        scheduler.Start();
        
        Assert.AreEqual("stub", result);
    }
    
    [TestMethod]
    public void observable_timeout_with_scheduler_timeout_error()
    {
        var scheduler = new TestScheduler();
        var stub = new SuccessHttpServiceStubWithScheduler
        {
            Scheduler = scheduler,
            Delay = TimeSpan.FromSeconds(1.5),
        };
        var my = new MyTimeoutClass(stub);
        Exception result = null;
        
        my.GetStringWithTimeout("http://www.example.com/", scheduler)
            .Subscribe(_ => Assert.Fail("Received value"), ex => { result = ex; });
        scheduler.Start();
        
        Assert.IsInstanceOfType(result, typeof(TimeoutException));
    }
}

class SuccessHttpServiceStub : IHttpService
{
    public IObservable<string> GetString(string url)
    {
        return Observable.Return("stub");
    }
}

class SuccessHttpServiceStubWithScheduler : IHttpService
{
    public IScheduler Scheduler { get; set; }
    public TimeSpan Delay { get; set; }
    public IObservable<string> GetString(string url)
    {
        return Observable.Return("stub")
            .Delay(Delay, Scheduler);
    }
}

class FailureHttpServiceStub : IHttpService
{
    public IObservable<string> GetString(string url)
    {
        return Observable.Throw<string>(new HttpRequestException());
    }
}
