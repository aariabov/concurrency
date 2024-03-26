using System.Net;
using System.Reactive.Linq;

namespace Tests.My.Rx;

[TestClass]
public class RxTests
{
    [TestMethod]
    public async Task observable_example()
    {
        IObservable<long> ticks = Observable.Timer(
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromSeconds(1));
        
        ticks.Subscribe(tick => Console.WriteLine($"Tick {tick}"));
        
        await Task.Delay(3100);
    }
}