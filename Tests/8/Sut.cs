using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Tests._8;

public class Sut
{
    public Task<string> DownloadStringTaskAsync(WebClient client, Uri address)
    {
        var tcs = new TaskCompletionSource<string>();
        // Обработка события завершит задачу и отменит свою регистрацию.
        DownloadStringCompletedEventHandler handler = null;
        handler = (_, e) =>
        {
            client.DownloadStringCompleted -= handler;
            if (e.Cancelled)
                tcs.TrySetCanceled();
            else if (e.Error != null)
                tcs.TrySetException(e.Error);
            else
                tcs.TrySetResult(e.Result);
        };
        // Зарегистрировать событие и *затем* начать операцию.
        client.DownloadStringCompleted += handler;
        client.DownloadStringAsync(address);
        return tcs.Task;
    }
    
    public Task<WebResponse> GetResponseAsync(WebRequest client)
    {
        return Task<WebResponse>.Factory.FromAsync(client.BeginGetResponse,
            client.EndGetResponse, null);
    }
    
    public IObservable<HttpResponseMessage> GetPage(HttpClient client, string url)
    {
        Task<HttpResponseMessage> task = client.GetAsync(url);
        return task.ToObservable();
    }
    
    public IObservable<HttpResponseMessage> GetPage1(HttpClient client, string url)
    {
        return Observable.StartAsync(token => client.GetAsync(url, token));
    }
    
    public IObservable<HttpResponseMessage> ColdGetPage(MockHttpClient client, string url)
    {
        return Observable.FromAsync(token => client.GetAsync(url, token));
    }
    
    public IObservable<HttpResponseMessage> GetPages(IObservable<string> urls, MockHttpClient client)
    {
        return urls.SelectMany((url, token) => client.GetAsync(url, token));
    }
}