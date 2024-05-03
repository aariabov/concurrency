namespace Tests;

public interface IRepository
{
    Task<int> GetCount();
}

public class SutService
{
    private readonly IRepository _repository;

    public SutService(IRepository repository = null)
    {
        _repository = repository;
    }

    public async Task<bool> IsExist()
    {
        return await Task.FromResult(false);
    }

    public async Task<int> GetCount()
    {
        return await _repository.GetCount();
    }
}