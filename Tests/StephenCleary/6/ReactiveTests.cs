using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Timers;
using Nito.AsyncEx;
using Timer = System.Timers.Timer;

namespace Tests._6;

[TestClass]
public class ReactiveTests
{
    [TestMethod]
    public async Task observable_example()
    {
        var timer = new Timer(interval: 1000) { Enabled = true };
        IObservable<EventPattern<ElapsedEventArgs>> ticks =
            Observable.FromEventPattern<ElapsedEventHandler, ElapsedEventArgs>(
                handler => (s, a) => handler(s, a),
                handler => timer.Elapsed += handler,
                handler => timer.Elapsed -= handler);
        ticks.Subscribe(data => Trace.WriteLine("OnNext:" + data.EventArgs.SignalTime));
        await Task.Delay(2000);
    }
    
    [TestMethod]
    public async Task observable_example1()
    {
        var timer = new Timer(interval: 1000) { Enabled = true };
        IObservable<EventPattern<object>> ticks = Observable.FromEventPattern(timer, nameof(Timer.Elapsed));
        ticks.Subscribe(data => Trace.WriteLine("OnNext: " + ((ElapsedEventArgs)data.EventArgs).SignalTime));
        await Task.Delay(2000);
    }
    
    [TestMethod]
    public async Task исключение_воспринимается_как_событие()
    {
        var client = new WebClient();
        IObservable<EventPattern<object>> downloadedStrings = Observable.FromEventPattern(client, nameof(WebClient.DownloadStringCompleted));
        downloadedStrings.Subscribe(
            data =>
            {
                var eventArgs = (DownloadStringCompletedEventArgs)data.EventArgs;
                if (eventArgs.Error != null)
                    Trace.WriteLine("OnNext: (Error) " + eventArgs.Error);
                else
                    Trace.WriteLine("OnNext: " + eventArgs.Result);
            },
            ex => Trace.WriteLine("OnError: " + ex.ToString()),
            () => Trace.WriteLine("OnCompleted"));
        client.DownloadStringAsync(new Uri("http://invalid.example.com/"));
        await Task.Delay(2000);
    }
    
    [TestMethod]
    public async Task события_возникают_в_разных_потоках()
    {
        AsyncContext.Run(async () =>
        {
            Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}");
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(x => Trace.WriteLine($"Interval {x} on thread {Environment.CurrentManagedThreadId}"));
            await Task.Delay(3100);
        });
    }
    
    [TestMethod]
    public async Task буфер_на_2_события()
    {
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Buffer(2)
            .Subscribe(x => Trace.WriteLine($"{DateTime.Now.Second}: Got {x[0]} and {x[1]}"));
        await Task.Delay(4100);
    }
    
    [TestMethod]
    public async Task группы_из_2_события()
    {
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Window(2)
            .Subscribe(group =>
            {
                Trace.WriteLine($"{DateTime.Now.Second}: Starting new group");
                group.Subscribe(
                    x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x}"),
                    () => Trace.WriteLine($"{DateTime.Now.Second}: Ending group"));
            });
        await Task.Delay(4100);
    }
    
    [TestMethod]
    public async Task timeout()
    {
        var client = new HttpClient();
        client.GetStringAsync("https://www.linkedin.com/")
            .ToObservable()
            .Timeout(TimeSpan.FromSeconds(1))
            .Subscribe(
                x => Trace.WriteLine($"{DateTime.Now.Second}: Saw {x.Length}"),
                ex => Trace.WriteLine(ex));
        await Task.Delay(1100);
    }
}