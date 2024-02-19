namespace Tests._2.Timeout;

public interface IRepository
{
    Task<int> GetCount();
    Task<int> GetCountWithTimeout(CancellationToken cancellationToken = default);
}

public class Sut
{
    private readonly IRepository _repository;

    public Sut(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> GetCount()
    {
        // Повторить попытку через 1 секунду, потом через 2
        TimeSpan nextDelay = TimeSpan.FromSeconds(1);
        for (int i = 0; i < 2; ++i)
        {
            try
            {
                return await _repository.GetCount();
            }
            catch
            {
            }
            await Task.Delay(nextDelay);
            nextDelay += nextDelay;
        }
        // Попробовать в последний раз и разрешить распространение ошибки.
        return await _repository.GetCount();
    }

    public async Task<int> GetCountWithTimeoutTest()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        Task timeoutTask = Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, cts.Token);
        Task<int> getCountTask = _repository.GetCountWithTimeout();
        
        Task completedTask = await Task.WhenAny(getCountTask, timeoutTask);
        if (completedTask == timeoutTask)
        {
            throw new TimeoutException();
        }
        
        return await getCountTask;
    }

    public async Task<int> GetCountWithTimeout(CancellationToken cancellationToken)
    {
        return await _repository.GetCountWithTimeout(cancellationToken);
    }
}