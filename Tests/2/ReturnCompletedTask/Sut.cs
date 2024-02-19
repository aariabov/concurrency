namespace Tests._2.ReturnCompletedTask;

public interface IRepository
{
    Task<int> GetCount(CancellationToken cancellationToken = default);
    Task Delete(CancellationToken cancellationToken = default);
}

public class Sut
{
    private readonly IRepository _repository;

    public Sut(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> GetCount(CancellationToken cancellationToken = default)
    {
        return await _repository.GetCount(cancellationToken);
    }

    public Task Delete(CancellationToken cancellationToken = default)
    {
        return _repository.Delete(cancellationToken);
    }
}