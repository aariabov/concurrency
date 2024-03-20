using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Tests.Reactive;

public interface IHttpService
{
    IObservable<string> GetString(string url);
}

public class MyTimeoutClass
{
    private readonly IHttpService _httpService;
    public MyTimeoutClass(IHttpService httpService)
    {
        _httpService = httpService;
    }
    public IObservable<string> GetStringWithTimeout(string url)
    {
        return _httpService
            .GetString(url)
            .Timeout(TimeSpan.FromSeconds(1));
    }
    
    public IObservable<string> GetStringWithTimeout(string url,
        IScheduler scheduler = null)
    {
        return _httpService.GetString(url)
            .Timeout(TimeSpan.FromSeconds(1), scheduler ?? Scheduler.Default);
    }
}