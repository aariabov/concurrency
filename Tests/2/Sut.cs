namespace Tests._2;

public interface IWebService
{
    Task<string> GetStringAsync();
}

public class Sut
{
    private readonly IWebService _webService;

    public Sut(IWebService webService)
    {
        _webService = webService;
    }

    public async Task<string> DownloadStringWithRetries()
    {
        // Повторить попытку через 1 секунду, потом через 2 и через 4 секунды.
        TimeSpan nextDelay = TimeSpan.FromSeconds(1);
        for (int i = 0; i < 3; ++i)
        {
            try
            {
                return await _webService.GetStringAsync();
            }
            catch
            {
            }
            await Task.Delay(nextDelay);
            nextDelay = nextDelay + nextDelay;
        }
        // Попробовать в последний раз и разрешить распространение ошибки.
        return await _webService.GetStringAsync();
    }
}