using System.Net;

namespace Tests.ShaktiTanwar._2TasksPattern;

[TestClass]
public class EventBasedAsynchronousTests
{
    [TestMethod]
    public async Task eap_to_task()
    {
        var result = await Task.Run(() =>
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                    taskCompletionSource.TrySetException(e.Error);
                else if (e.Cancelled)
                    taskCompletionSource.TrySetCanceled();
                else
                    taskCompletionSource.TrySetResult(e.Result);
            };
            webClient.DownloadStringAsync(new Uri("https://google.com"));
            return taskCompletionSource.Task;
        });

        Assert.IsTrue(result.Length > 0);
    }
}