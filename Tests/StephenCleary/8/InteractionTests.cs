using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualBasic;

namespace Tests._8;

[TestClass]
public class InteractionTests
{
    [TestMethod]
    public async Task webclient_return_result()
    {
        var sut = new Sut();
        var webClient = new WebClient();
        var uri = new Uri("http://ya.ru/favicon.ico");

        var result = await sut.DownloadStringTaskAsync(webClient, uri);

        Assert.IsTrue(result.Length > 0);
    }
    
    [TestMethod]
    public async Task webclient_error()
    {
        var sut = new Sut();
        var webClient = new WebClient();
        var uri = new Uri("http://invalid.example.com/");

        var task = sut.DownloadStringTaskAsync(webClient, uri);

        var func = async () => await task;
        await Assert.ThrowsExceptionAsync<WebException>(func);
        Assert.IsTrue(task.IsFaulted);
    }
    
    [TestMethod]
    public async Task web_request_return_result()
    {
        var sut = new Sut();
        var webRequest = WebRequest.Create("http://ya.ru/favicon.ico");

        var result = await sut.GetResponseAsync(webRequest);

        Assert.IsTrue(result.ContentLength > 0);
    }
    
    [TestMethod]
    public async Task hot_get_async_as_observable_ok()
    {
        var sut = new Sut();
        var client = new HttpClient();

        // горячий запуск: запускается сразу, не ожидая подписки
        var result = sut.GetPage(client, "http://ya.ru/");
        result.Subscribe(
            x =>
            {
                Console.WriteLine($"Result status: {x.StatusCode}");
                Assert.AreEqual(HttpStatusCode.OK, x.StatusCode);
            },
            ex =>
            {
                Console.WriteLine(ex);
                Assert.Fail("Должно работать");
            });
        
        await Task.Delay(1100);
    }
    
    [TestMethod]
    public async Task hot_get_async_as_observable_ok1()
    {
        var sut = new Sut();
        var client = new HttpClient();

        // горячий запуск: запускается сразу, не ожидая подписки
        var result = sut.GetPage1(client, "http://ya.ru/");
        result.Subscribe(x => Assert.AreEqual(HttpStatusCode.OK, x.StatusCode));
        
        await Task.Delay(1100);
    }
    
    [TestMethod]
    public async Task cold_get_async_as_observable_no_call()
    {
        var sut = new Sut();
        var client = new MockHttpClient();

        // холодный запуск: НЕ запускается, пока не будет подписки
        var result = sut.ColdGetPage(client, "http://ya.ru/");
        
        await Task.Delay(1100);
        Assert.AreEqual(0, client.GetAsyncCallTimes);
    }
    
    [TestMethod]
    public async Task cold_get_async_as_observable_with_call()
    {
        var sut = new Sut();
        var client = new MockHttpClient();

        // холодный запуск: НЕ запускается, пока не будет подписки
        var result = sut.ColdGetPage(client, "http://ya.ru/");
        result.Subscribe(x => Assert.AreEqual(HttpStatusCode.OK, x.StatusCode));
        result.Subscribe(x => Assert.AreEqual(HttpStatusCode.OK, x.StatusCode));
        
        await Task.Delay(1100);
        Assert.AreEqual(2, client.GetAsyncCallTimes);
    }
    
    [TestMethod]
    public async Task get_observable_pages()
    {
        var sut = new Sut();
        var client = new MockHttpClient();
        var urls = new[] { "http://ya.ru/", "http://google.com/" }.ToObservable();

        var result = sut.GetPages(urls, client);
        result.Subscribe(x => Assert.AreEqual(HttpStatusCode.OK, x.StatusCode));
        result.Subscribe(x => Assert.AreEqual(HttpStatusCode.OK, x.StatusCode));
        
        await Task.Delay(1100);
        Assert.AreEqual(4, client.GetAsyncCallTimes);
    }
    
    [TestMethod]
    public async Task data_flow_as_async()
    {
        var multiplyBlock = new TransformBlock<int, int>(value => value * 2);
        multiplyBlock.Post(5);
        multiplyBlock.Post(2);
        multiplyBlock.Complete();
        var result = multiplyBlock.ReceiveAllAsync();
        Assert.AreEqual(10, await result.FirstAsync());
        Assert.AreEqual(4, await result.LastAsync());
        // await foreach (int item in multiplyBlock.ReceiveAllAsync())
        // {
        //     Console.WriteLine(item);
        // }
    }
    
    [TestMethod]
    public async Task buffer_block_as_observable()
    {
        var buffer = new BufferBlock<int>();
        IObservable<int> integers = buffer.AsObservable();
        integers.Subscribe(data =>
            {
                Trace.WriteLine(data);
                Assert.AreEqual(13, data);
            },
            ex => Trace.WriteLine(ex),
            () => Trace.WriteLine("Done"));
        buffer.Post(13);
        await Task.Delay(10);
    }
    
    [TestMethod]
    public async Task action_block_as_a_observer()
    {
        IObservable<DateTimeOffset> ticks =
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Timestamp()
                .Select(x => x.Timestamp)
                .Take(5);
        
        var display = new ActionBlock<DateTimeOffset>(x => Trace.WriteLine(x));
        ticks.Subscribe(display.AsObserver());
        try
        {
            display.Completion.Wait();
            Trace.WriteLine("Done.");
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    [TestMethod]
    public async Task observable_to_async_enumerable()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));
        // ПРЕДУПРЕЖДЕНИЕ: может потреблять неограниченную память
        IAsyncEnumerable<long> enumerable = observable.ToAsyncEnumerable().Take(3);
        await foreach (var e in enumerable)
        {
            Trace.WriteLine(e);
        }
    }

    [TestMethod]
    public async Task observable_to_custom_async_enumerable()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));
        // ПРЕДУПРЕЖДЕНИЕ: может потреблять неограниченную память
        IAsyncEnumerable<long> enumerable = observable.ToCustomAsyncEnumerable().Take(3);
        await foreach (var e in enumerable)
        {
            Trace.WriteLine(e);
        }
    }

    [TestMethod]
    public async Task observable_to_buffer_async_enumerable()
    {
        IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1));
        // ПРЕДУПРЕЖДЕНИЕ: потеря елементов, тк продьюсер производит элементы быстрее,
        // чем консамер может их потребить
        IAsyncEnumerable<long> enumerable = observable
            .ToBufferAsyncEnumerable(2)
            .Take(5);
        await foreach (var e in enumerable)
        {
            await Task.Delay(2000);
            Trace.WriteLine(e);
        }
    }

}
    
public class MockHttpClient : HttpClient
{
    public int GetAsyncCallTimes { get; set; }
        
    public new Task<HttpResponseMessage> GetAsync(string? requestUri, CancellationToken cancellationToken)
    {
        GetAsyncCallTimes++;
        return base.GetAsync(requestUri, cancellationToken);
    }
}