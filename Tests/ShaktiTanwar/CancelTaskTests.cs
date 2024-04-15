using System.Net;

namespace Tests.ShaktiTanwar;

[TestClass]
public class CancelTaskTests
{
    [TestMethod]
    public async Task cancel_web_client()
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        DownloadFileWithToken(token);
        await Task.Delay(200);
        cancellationTokenSource.Cancel();
        await Task.Delay(200);
    }
    
    private static void DownloadFileWithToken(CancellationToken token)
    {
        WebClient webClient = new WebClient();
        //Here we are registering callback delegate that will get called as soon as user cancels token
        token.Register(() => webClient.CancelAsync());

        webClient.DownloadStringAsync(new Uri("http://www.google.com"));
        webClient.DownloadStringCompleted += (sender, e) => {
            if (!e.Cancelled)
            {
                Console.WriteLine("Download Complete.");
            }
            else
            {
                Console.WriteLine("Download Cancelled.");
            }
        };
    }
    
    [TestMethod]
    public async Task cancel_long_loop()
    {
        var cancellationTokenSource = new CancellationTokenSource(500);
        var token = cancellationTokenSource.Token;
        
        var func = () => LongLoop(token);
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(func);
    }

    private async Task LongLoop(CancellationToken token)
    {
        for (int i = 0; i < 1000; i++)
        {
            Console.WriteLine($"Iter {i}");
            await Task.Delay(100, token);

            if (token.IsCancellationRequested)
            {
                Console.WriteLine($"Task was canceled");
                token.ThrowIfCancellationRequested();
            }
        }
    }
}