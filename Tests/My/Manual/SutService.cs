using System.Net;

namespace Tests.My.Manual;

public interface IRepository
{
    void GetCount(Action<int> callback);
    void BeginGetCountAsyncCallback(AsyncCallback? callback, object unrelatedObject);
    int EndGetCountAsyncCallback(IAsyncResult result);
    
    void GetCountOnEvent();
    public event GetCountEventHandler? GetCountCompleted;
}

public class SutService
{
    private readonly IRepository _repository;

    public SutService(IRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Имитация обработчика кнопки: запускает длительную операцию и передает ей callback для обновления интерфейса
    /// </summary>
    public void UpdateCount_BtnClick_Callback()
    {
        Console.WriteLine($"Main start: {Thread.CurrentThread.ManagedThreadId}");
        _repository.GetCount((count) =>
        {
            // обновляем интерфейс
            Console.WriteLine($"Callback {count} шт.: {Thread.CurrentThread.ManagedThreadId}");
        });
        Console.WriteLine($"Main end: {Thread.CurrentThread.ManagedThreadId}");
    }

    /// <summary>
    /// Имитация обработчика кнопки: запускает длительную операцию и подписывается на событие для обновления интерфейса
    /// Недостатки: метод разбирается на 2, надо не забыть отписаться от события
    /// </summary>
    public void UpdateCount_BtnClick_Event()
    {
        Console.WriteLine($"Main start: {Thread.CurrentThread.ManagedThreadId}");
        _repository.GetCountCompleted += (sender, args) =>
        {
            // обновляем интерфейс
            Console.WriteLine($"Callback {args.Count} шт.: {Thread.CurrentThread.ManagedThreadId}");
        };
        _repository.GetCountOnEvent();
        Console.WriteLine($"Main end: {Thread.CurrentThread.ManagedThreadId}");
    }

    /// <summary>
    /// Имитация обработчика кнопки: запускает длительную операцию и использует AsyncResult
    /// Недостатки: метод разбивается на 2, выглядит все не очень
    /// </summary>
    public void UpdateCount_BtnClick_AsyncResult()
    {
        Console.WriteLine($"Main start: {Thread.CurrentThread.ManagedThreadId}");
        object unrelatedObject = "hello";
        // запускает длительную операцию
        _repository.BeginGetCountAsyncCallback(OnResolved, unrelatedObject);
        Console.WriteLine($"Main end: {Thread.CurrentThread.ManagedThreadId}");
    }
    
    private void OnResolved(IAsyncResult ar)
    {
        object unrelatedObject = ar.AsyncState;
        // получает результат
        var result = _repository.EndGetCountAsyncCallback(ar);
        // Обработать адрес
        Console.WriteLine($"Callback {result}шт. {unrelatedObject}: {Thread.CurrentThread.ManagedThreadId}");
    }
}